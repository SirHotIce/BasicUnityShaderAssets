# pc_server.py
import socket, json

HOST = "127.0.0.1"
PORT = 7000

def recv_line(conn):
    buf = bytearray()
    while True:
        b = conn.recv(1)
        if not b:
            return None
        if b == b"\n":
            return buf.decode("utf-8", errors="replace")
        buf.extend(b)

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
    s.bind((HOST, PORT))
    s.listen(1)
    print(f"Listening on {HOST}:{PORT}")

    while True:
        conn, addr = s.accept()
        print("Client connected:", addr)
        with conn:
            while True:
                line = recv_line(conn)
                if line is None:
                    print("Client disconnected")
                    break
                try:
                    msg = json.loads(line)
                    print("RX:", msg)
                    reply = {"ok": True, "echo": msg}
                except Exception as e:
                    reply = {"ok": False, "error": str(e)}
                conn.sendall((json.dumps(reply) + "\n").encode("utf-8"))