using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UdpGestureReceiverV2 : MonoBehaviour
{
    public enum HandsUpState { None = 0, Right = 1, Left = 2, Both = 3 }

    [Header("UDP Listen")]
    public int port = 5052;

    [Header("Outputs")]
    public HandsUpState handsUpState;
    public bool armsCrossed;
    public bool crossPressedThisFrame;

    [Header("Debug")]
    public bool showOnScreenDebug = true;
    public bool logOnChange = true;

    private UdpClient _client;
    private Thread _thread;
    private volatile bool _running;

    // thread-safe raw (volatile int is fine)
    private volatile int _x;
    private volatile int _h;

    // main-thread edge detect
    private bool _prevCross;

    // debug info (use Interlocked for long)
    private long _lastRecvTicks;          // DO NOT make volatile
    private volatile int _lastBytes;

    private string _lastPacket = "";
    private readonly object _packetLock = new object();

    private HandsUpState _prevHandsLogged = HandsUpState.None;
    private bool _prevCrossLogged = false;

    void Start()
    {
        try
        {
            _running = true;

            _client = new UdpClient(new IPEndPoint(IPAddress.Any, port));
            _client.Client.ReceiveTimeout = 500;

            _thread = new Thread(ListenLoop) { IsBackground = true };
            _thread.Start();

            Debug.Log($"UdpGestureReceiverV2 listening on UDP {port}");
        }
        catch (Exception e)
        {
            Debug.LogError($"UdpGestureReceiverV2 failed to bind port {port}: {e}");
            enabled = false;
        }
    }

    void Update()
    {
        handsUpState = (HandsUpState)_h;
        armsCrossed = (_x == 1);

        crossPressedThisFrame = armsCrossed && !_prevCross;
        _prevCross = armsCrossed;

        if (logOnChange)
        {
            if (handsUpState != _prevHandsLogged || armsCrossed != _prevCrossLogged || crossPressedThisFrame)
            {
                Debug.Log($"[UDP] H={handsUpState} X={(armsCrossed ? 1 : 0)} pressed={crossPressedThisFrame} ageMs={LastAgeMs()}");
                _prevHandsLogged = handsUpState;
                _prevCrossLogged = armsCrossed;
            }
        }
    }

    private void ListenLoop()
    {
        IPEndPoint ep = new IPEndPoint(IPAddress.Any, port);

        while (_running)
        {
            try
            {
                byte[] data = _client.Receive(ref ep);

                _lastBytes = data.Length;
                Interlocked.Exchange(ref _lastRecvTicks, DateTime.UtcNow.Ticks);

                string s = Encoding.UTF8.GetString(data);
                lock (_packetLock) _lastPacket = s;

                Parse(s);
            }
            catch (SocketException)
            {
                // timeout or socket closed
            }
            catch (ObjectDisposedException)
            {
                // during shutdown
            }
            catch
            {
                // ignore malformed packets
            }
        }
    }

    private void Parse(string s)
    {
        var parts = s.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var p in parts)
        {
            if (p.StartsWith("X="))
            {
                _x = (p.Length > 2 && p[2] == '1') ? 1 : 0;
            }
            else if (p.StartsWith("H="))
            {
                var v = p.Substring(2);
                if (v == "NONE") _h = 0;
                else if (v == "RIGHT") _h = 1;
                else if (v == "LEFT") _h = 2;
                else if (v == "BOTH") _h = 3;
            }
        }
    }

    private long LastAgeMs()
    {
        long ticks = Interlocked.Read(ref _lastRecvTicks);
        if (ticks == 0) return 999999;
        long dtTicks = DateTime.UtcNow.Ticks - ticks;
        return dtTicks / TimeSpan.TicksPerMillisecond;
    }

    void OnGUI()
    {
        if (!showOnScreenDebug) return;

        string pkt;
        lock (_packetLock) pkt = _lastPacket;

        GUI.Label(new Rect(10, 10, 900, 20), $"UDP {port} lastAgeMs={LastAgeMs()} bytes={_lastBytes}");
        GUI.Label(new Rect(10, 30, 900, 20), $"Parsed: H={handsUpState} X={(armsCrossed ? 1 : 0)} pressed={crossPressedThisFrame}");
        GUI.Label(new Rect(10, 50, 1200, 20), $"Last packet: {pkt}");
    }

    void OnApplicationQuit()
    {
        _running = false;
        try { _client?.Close(); } catch { }
        try { _thread?.Join(200); } catch { }
    }
}
