import os, time, math, socket, urllib.request
import cv2
import mediapipe as mp

# ===================== UDP -> Unity =====================
UNITY_IP = "127.0.0.1"
UNITY_PORT = 5052

# ===================== BEHAVIOR =====================
BOTH_DELAY_S = 1.0          # wait this long before committing RIGHT/LEFT so BOTH can win
X_STICKY_MS = 500           # 0.5s buffer for cross flicker

# Hand-up flicker fix (hysteresis around the shoulder line)
HAND_ON_MARGIN  = 0.010     # wrist must be above shoulder by this to turn ON
HAND_OFF_MARGIN = 0.020     # wrist must drop below shoulder by this to turn OFF

# Cross less strict: intersect OR "near" (distance threshold relative to shoulder width)
CROSS_NEAR_FACTOR = 0.45    # bigger => easier cross (0.35..0.60 typical)

# Pose drop handling (avoid snapping to NONE if tracking blips)
POSE_LOST_RESET_S = 0.80    # after this long without pose, reset hands to NONE

# ===================== MODEL =====================
MODEL_URL = (
    "https://storage.googleapis.com/mediapipe-models/pose_landmarker/"
    "pose_landmarker_lite/float16/latest/pose_landmarker_lite.task"
)
MODEL_PATH = os.path.join(os.path.dirname(__file__), "pose_landmarker_lite.task")

# ===================== VISUAL DEBUG =====================
DRAW_STICK_FIGURE = True
MIRROR_DISPLAY = True  # show selfie-mirrored preview ONLY; logic/UDP stays unmirrored

# BlazePose landmark indices (33 landmarks)
NOSE = 0
L_SHO, R_SHO = 11, 12
L_ELB, R_ELB = 13, 14
L_WRI, R_WRI = 15, 16
L_HIP, R_HIP = 23, 24
L_KNE, R_KNE = 25, 26
L_ANK, R_ANK = 27, 28

POSE_CONNECTIONS = [
    (NOSE, L_SHO), (NOSE, R_SHO),
    (L_SHO, R_SHO),
    (L_SHO, L_HIP), (R_SHO, R_HIP),
    (L_HIP, R_HIP),
    (L_SHO, L_ELB), (L_ELB, L_WRI),
    (R_SHO, R_ELB), (R_ELB, R_WRI),
    (L_HIP, L_KNE), (L_KNE, L_ANK),
    (R_HIP, R_KNE), (R_KNE, R_ANK),
]

def ensure_model(path: str):
    if os.path.exists(path) and os.path.getsize(path) > 0:
        return
    os.makedirs(os.path.dirname(path), exist_ok=True)
    urllib.request.urlretrieve(MODEL_URL, path)

def lm_xy(lm):
    return (lm.x, lm.y)  # normalized

def dist(a, b):
    return math.hypot(a[0]-b[0], a[1]-b[1])

def clamp01(v: float) -> float:
    return 0.0 if v < 0.0 else (1.0 if v > 1.0 else v)

def lm_to_px(lm, w: int, h: int, mirror_x: bool):
    x_norm = clamp01(lm.x)
    if mirror_x:
        x_norm = 1.0 - x_norm
    x = int(x_norm * (w - 1))
    y = int(clamp01(lm.y) * (h - 1))
    return (x, y)

def lm_vis(lm) -> float:
    return float(getattr(lm, "visibility", 1.0) if getattr(lm, "visibility", None) is not None else 1.0)

def draw_stick_figure(frame_bgr, lms, connections, mirror_x: bool, vis_thresh: float = 0.35):
    h, w = frame_bgr.shape[:2]
    thick = max(2, int(min(w, h) * 0.004))
    r = max(2, int(min(w, h) * 0.006))

    for a, b in connections:
        if a >= len(lms) or b >= len(lms):
            continue
        la, lb = lms[a], lms[b]
        if lm_vis(la) < vis_thresh or lm_vis(lb) < vis_thresh:
            continue
        pa = lm_to_px(la, w, h, mirror_x)
        pb = lm_to_px(lb, w, h, mirror_x)
        cv2.line(frame_bgr, pa, pb, (0, 255, 0), thick, cv2.LINE_AA)

    used = set()
    for a, b in connections:
        used.add(a); used.add(b)
    for i in used:
        if i >= len(lms):
            continue
        li = lms[i]
        if lm_vis(li) < vis_thresh:
            continue
        p = lm_to_px(li, w, h, mirror_x)
        cv2.circle(frame_bgr, p, r, (0, 255, 0), -1, cv2.LINE_AA)

# ---------- geometry: robust-ish segment intersection + segment distance ----------
def orient(p, q, r):
    return (q[0]-p[0])*(r[1]-p[1]) - (q[1]-p[1])*(r[0]-p[0])

def on_segment(p, q, r, eps):
    return (min(p[0], r[0]) - eps <= q[0] <= max(p[0], r[0]) + eps and
            min(p[1], r[1]) - eps <= q[1] <= max(p[1], r[1]) + eps)

def segments_intersect_eps(a, b, c, d, eps=1e-6):
    o1 = orient(a,b,c); o2 = orient(a,b,d); o3 = orient(c,d,a); o4 = orient(c,d,b)
    if (((o1 > eps and o2 < -eps) or (o1 < -eps and o2 > eps)) and
        ((o3 > eps and o4 < -eps) or (o3 < -eps and o4 > eps))):
        return True
    if abs(o1) <= eps and on_segment(a,c,b,eps): return True
    if abs(o2) <= eps and on_segment(a,d,b,eps): return True
    if abs(o3) <= eps and on_segment(c,a,d,eps): return True
    if abs(o4) <= eps and on_segment(c,b,d,eps): return True
    return False

def point_seg_dist(p, a, b):
    ax, ay = a; bx, by = b; px, py = p
    abx, aby = bx-ax, by-ay
    apx, apy = px-ax, py-ay
    denom = abx*abx + aby*aby
    if denom < 1e-12:
        return math.hypot(px-ax, py-ay)
    t = (apx*abx + apy*aby) / denom
    t = max(0.0, min(1.0, t))
    cx, cy = ax + t*abx, ay + t*aby
    return math.hypot(px-cx, py-cy)

def seg_seg_dist(a, b, c, d):
    if segments_intersect_eps(a, b, c, d, eps=1e-5):
        return 0.0
    return min(
        point_seg_dist(a, c, d),
        point_seg_dist(b, c, d),
        point_seg_dist(c, a, b),
        point_seg_dist(d, a, b),
    )

# ---------- hand-up hysteresis ----------
class HandHysteresis:
    def __init__(self):
        self.right_up = False
        self.left_up = False

    def update(self, rwri_y, rsho_y, lwri_y, lsho_y):
        if not self.right_up:
            if rwri_y < (rsho_y - HAND_ON_MARGIN):
                self.right_up = True
        else:
            if rwri_y > (rsho_y + HAND_OFF_MARGIN):
                self.right_up = False

        if not self.left_up:
            if lwri_y < (lsho_y - HAND_ON_MARGIN):
                self.left_up = True
        else:
            if lwri_y > (lsho_y + HAND_OFF_MARGIN):
                self.left_up = False

        return self.right_up, self.left_up

# ---------- no flicker state machine for H ----------
class HandsStateMachine:
    # Outputs: "NONE", "RIGHT", "LEFT", "BOTH"
    def __init__(self, both_delay_s: float):
        self.delay_s = both_delay_s
        self.state = "IDLE"          # IDLE, PEND_R, PEND_L, HOLD_R, HOLD_L, HOLD_B
        self.t_start = 0.0

    def update(self, r_up: bool, l_up: bool, now: float) -> str:
        if r_up and l_up:
            self.state = "HOLD_B"
            return "BOTH"

        if self.state == "IDLE":
            if r_up and not l_up:
                self.state = "PEND_R"; self.t_start = now
                return "NONE"
            if l_up and not r_up:
                self.state = "PEND_L"; self.t_start = now
                return "NONE"
            return "NONE"

        if self.state == "PEND_R":
            if not r_up and not l_up:
                self.state = "IDLE"; return "NONE"
            if l_up and not r_up:
                self.state = "PEND_L"; self.t_start = now
                return "NONE"
            if now - self.t_start >= self.delay_s:
                self.state = "HOLD_R"
                return "RIGHT"
            return "NONE"

        if self.state == "PEND_L":
            if not r_up and not l_up:
                self.state = "IDLE"; return "NONE"
            if r_up and not l_up:
                self.state = "PEND_R"; self.t_start = now
                return "NONE"
            if now - self.t_start >= self.delay_s:
                self.state = "HOLD_L"
                return "LEFT"
            return "NONE"

        if self.state == "HOLD_R":
            if r_up and not l_up:
                return "RIGHT"
            if not r_up and not l_up:
                self.state = "IDLE"; return "NONE"
            if l_up and not r_up:
                self.state = "PEND_L"; self.t_start = now
                return "NONE"
            return "NONE"

        if self.state == "HOLD_L":
            if l_up and not r_up:
                return "LEFT"
            if not r_up and not l_up:
                self.state = "IDLE"; return "NONE"
            if r_up and not l_up:
                self.state = "PEND_R"; self.t_start = now
                return "NONE"
            return "NONE"

        if self.state == "HOLD_B":
            if r_up and not l_up:
                self.state = "PEND_R"; self.t_start = now
                return "NONE"
            if l_up and not r_up:
                self.state = "PEND_L"; self.t_start = now
                return "NONE"
            self.state = "IDLE"
            return "NONE"

        self.state = "IDLE"
        return "NONE"

def main():
    ensure_model(MODEL_PATH)
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)

    BaseOptions = mp.tasks.BaseOptions
    PoseLandmarker = mp.tasks.vision.PoseLandmarker
    PoseLandmarkerOptions = mp.tasks.vision.PoseLandmarkerOptions
    RunningMode = mp.tasks.vision.RunningMode

    options = PoseLandmarkerOptions(
        base_options=BaseOptions(model_asset_path=MODEL_PATH),
        running_mode=RunningMode.VIDEO,
        num_poses=1,
        min_pose_detection_confidence=0.3,
        min_pose_presence_confidence=0.3,
        min_tracking_confidence=0.3
    )

    cap = cv2.VideoCapture(0)
    if not cap.isOpened():
        raise RuntimeError("Could not open webcam")

    hand_hys = HandHysteresis()
    hands_fsm = HandsStateMachine(BOTH_DELAY_S)

    x_until = 0.0
    last_pose_t = -1e9
    t0 = time.perf_counter()

    with PoseLandmarker.create_from_options(options) as landmarker:
        while True:
            ok, frame_bgr = cap.read()
            if not ok:
                break

            # ---- detection runs on the ORIGINAL (unmirrored) frame ----
            frame_rgb = cv2.cvtColor(frame_bgr, cv2.COLOR_BGR2RGB)
            mp_image = mp.Image(image_format=mp.ImageFormat.SRGB, data=frame_rgb)
            ts_ms = int((time.perf_counter() - t0) * 1000)
            result = landmarker.detect_for_video(mp_image, ts_ms)

            now = time.perf_counter()
            have_pose = (result.pose_landmarks and len(result.pose_landmarks) > 0)
            x_raw = False

            if have_pose:
                last_pose_t = now
                lms = result.pose_landmarks[0]

                # logic uses original landmarks (unmirrored)
                rsho = lm_xy(lms[R_SHO]); rwri = lm_xy(lms[R_WRI])
                lsho = lm_xy(lms[L_SHO]); lwri = lm_xy(lms[L_WRI])
                relb = lm_xy(lms[R_ELB]); lelb = lm_xy(lms[L_ELB])

                shoulder_w = max(1e-6, dist(lsho, rsho))
                cross_tol = CROSS_NEAR_FACTOR * shoulder_w

                r_up, l_up = hand_hys.update(rwri[1], rsho[1], lwri[1], lsho[1])

                x_raw = (
                    segments_intersect_eps(lelb, lwri, relb, rwri, eps=1e-5)
                    or (seg_seg_dist(lelb, lwri, relb, rwri) < cross_tol)
                )
            else:
                if (now - last_pose_t) > POSE_LOST_RESET_S:
                    hand_hys.right_up = False
                    hand_hys.left_up = False
                    hands_fsm.state = "IDLE"
                r_up, l_up = hand_hys.right_up, hand_hys.left_up

            if x_raw:
                x_until = now + (X_STICKY_MS / 1000.0)
            x = (now < x_until)

            H = hands_fsm.update(r_up, l_up, now)

            msg = f"H={H} X={1 if x else 0}"
            sock.sendto(msg.encode("utf-8"), (UNITY_IP, UNITY_PORT))

            # ---- display can be mirrored without affecting logic ----
            disp = cv2.flip(frame_bgr, 1) if MIRROR_DISPLAY else frame_bgr

            if have_pose and DRAW_STICK_FIGURE:
                # draw landmarks mirrored to match the mirrored preview
                draw_stick_figure(disp, lms, POSE_CONNECTIONS, mirror_x=MIRROR_DISPLAY)

            # draw text AFTER mirroring so it reads normally
            cv2.putText(disp, msg, (10, 35), cv2.FONT_HERSHEY_SIMPLEX, 1.0,
                        (255, 255, 255), 2, cv2.LINE_AA)

            cv2.imshow("Python -> Unity UDP", disp)
            if (cv2.waitKey(1) & 0xFF) == ord("q"):
                break

    cap.release()
    cv2.destroyAllWindows()
    sock.close()

if __name__ == "__main__":
    main()