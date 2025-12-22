using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UsbTcpJsonClient : MonoBehaviour
{
    public async void SendTest()
    {
        // With adb reverse, the phone connects to its own localhost:7000
        string host = "127.0.0.1";
        int port = 7000;

        var payload = new { type = "ping", time = DateTime.UtcNow.ToString("o") };
        string json = JsonUtility.ToJson(new Wrapper(payload)); // see wrapper below

        try
        {
            using var client = new TcpClient();
            await client.ConnectAsync(host, port);

            using NetworkStream stream = client.GetStream();
            byte[] outBytes = Encoding.UTF8.GetBytes(json + "\n");
            await stream.WriteAsync(outBytes, 0, outBytes.Length);

            // Read reply line
            string reply = await ReadLineAsync(stream);
            Debug.Log("Reply: " + reply);
        }
        catch (Exception e)
        {
            Debug.LogError("TCP failed: " + e);
        }
    }

    private static async Task<string> ReadLineAsync(NetworkStream stream)
    {
        var sb = new StringBuilder();
        var buf = new byte[1];
        while (true)
        {
            int n = await stream.ReadAsync(buf, 0, 1);
            if (n == 0) return null;
            char c = (char)buf[0];
            if (c == '\n') return sb.ToString();
            sb.Append(c);
        }
    }

    // JsonUtility can't serialize anonymous objects directly, so wrap.
    [Serializable]
    private class Wrapper
    {
        public string type;
        public string time;

        public Wrapper(object obj)
        {
            var t = obj.GetType();
            type = (string)t.GetProperty("type")?.GetValue(obj);
            time = (string)t.GetProperty("time")?.GetValue(obj);
        }
    }
}