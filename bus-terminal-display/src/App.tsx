import { useState, useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { domToPng } from "modern-screenshot";
import "./App.css";

interface StationData {
  stationId: string;
  stationName: string;
  updateTime: any;
  busList: BusArrivalData[];
}

interface BusArrivalData {
  routeId: string;
  routeNo: string;
  routeDirection: string;
  currSttnName: string | null;
  operationMode: number;
  busType: number;
  predictType: number;
  remainStop: number;
  remainTime: number;
}

function App() {
  const STATION_ID = 3399003; // Hardcoded default station
  const BACKEND_URL = "https://localhost:7113/bitHub"; // Update with your API URL
  const RUST_WS_URL = "ws://127.0.0.1:3000/ws";

  const [connectionStatus, setConnectionStatus] =
    useState<string>("Disconnected");
  const [arrivals, setArrivals] = useState<StationData>();
  const [stationName, setStationName] = useState<string>(
    `Station ${STATION_ID}`,
  );
  const [battery, setBattery] = useState<number | null>(null);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const rustWsRef = useRef<WebSocket | null>(null);
  const displayRef = useRef<HTMLDivElement | null>(null);

  // useEffect(() => {
  //   function connectToDaemon() {
  //     const ws = new WebSocket(RUST_WS_URL);
  //     rustWsRef.current = ws;

  //     ws.onmessage = (event) => {
  //       try {
  //         const telemetry = JSON.parse(event.data);
  //         if (telemetry.battery !== undefined) {
  //           setBattery(telemetry.battery); // Instant visual update when event fires
  //         }
  //       } catch (err) {
  //         console.error("Failed parsing incoming telemetry packet:", err);
  //       }
  //     };

  //     ws.onclose = () => {
  //       console.warn(
  //         "Daemon WS closed. Attempting reconnect event loop in 5 seconds...",
  //       );
  //       setTimeout(connectToDaemon, 5000);
  //     };
  //   }

  //   connectToDaemon();
  //   return () => rustWsRef.current?.close();
  // }, []);

  // useEffect(() => {
  //   if (arrivals?.busList.length === 0 || !displayRef.current) return;

  //   const timer = setTimeout(async () => {
  //     try {
  //       if (rustWsRef.current?.readyState === WebSocket.OPEN) {
  //         const dataUrl = await domToPng(displayRef.current!, { quality: 1 });
  //         const base64Data = dataUrl.split(",")[1]; // Get raw Base64 contents strictly

  //         // Stream the data directly over the open channel socket
  //         rustWsRef.current.send(
  //           JSON.stringify({
  //             station_id: STATION_ID,
  //             image_base64: base64Data,
  //           }),
  //         );
  //       }
  //     } catch (err) {
  //       console.error(
  //         "Failed capturing or dispatching layout stream frame:",
  //         err,
  //       );
  //     }
  //   }, 300);

  //   return () => clearTimeout(timer);
  // }, [arrivals]);

  useEffect(() => {
    if (!connectionRef.current) {
      connectionRef.current = new signalR.HubConnectionBuilder()
        .withUrl(BACKEND_URL)
        .withAutomaticReconnect()
        .build();
    }

    const connection = connectionRef.current;

    connection.on("ReceiveBusArrivals", (data: StationData) => {
      setArrivals(data);
      if (data.busList.length > 0 && data.stationName)
        setStationName(data.stationName);
    });

    connection.onreconnected(async () => {
      setConnectionStatus("Connected");
      await connection.invoke("JoinStation", STATION_ID.toString());
    });

    async function startConnection() {
      try {
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
          return;
        }

        await connection.start();
        setConnectionStatus("Connected");
        await connection.invoke("JoinStation", STATION_ID.toString());
      } catch {
        setConnectionStatus("Connection Failed");
        setTimeout(startConnection, 5000);
      }
    }

    startConnection();
    return () => {
      if (
        connection &&
        connection.state === signalR.HubConnectionState.Connected
      ) {
        connection
          .stop()
          .then(() => {
            connectionRef.current = null;
          })
          .catch((err) => console.error("Error stopping connection:", err));
      }
    };
  }, []);

  return (
    <div
      ref={displayRef}
      style={{
        padding: "20px",
        fontFamily: "monospace",
        backgroundColor: "#121212",
        color: "#fff",
        minHeight: "100vh",
      }}
    >
      {/* Top Banner Status Bar */}
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          borderBottom: "2px solid #333",
          paddingBottom: "10px",
        }}
      >
        <h2>DEPARTURE BOARD: {stationName.toUpperCase()}</h2>
        <div style={{ display: "flex", gap: "20px" }}>
          {battery !== null && <span>⚡ Battery: {battery}%</span>}
          <span>SignalR: {connectionStatus}</span>
        </div>
      </div>

      <table
        style={{
          width: "100%",
          textAlign: "left",
          marginTop: "20px",
          fontSize: "1.4rem",
          borderCollapse: "collapse",
        }}
      >
        <thead>
          <tr style={{ color: "#aaa", borderBottom: "1px solid #444" }}>
            <th style={{ padding: "12px" }}>ROUTE</th>
            <th>DESTINATION</th>
            <th style={{ textAlign: "right" }}>STOPS AWAY</th>
            <th style={{ textAlign: "right", paddingRight: "12px" }}>
              ARRIVING
            </th>
          </tr>
        </thead>
        <tbody>
          {arrivals?.busList.map((bus, idx) => (
            <tr
              key={idx}
              style={{
                borderBottom: "1px solid #222",
                color: bus.remainTime <= 3 ? "#ff9800" : "#fff",
              }}
            >
              <td style={{ padding: "16px", fontWeight: "bold" }}>
                {bus.routeNo}
              </td>
              <td>{bus.routeDirection}</td>
              <td style={{ textAlign: "right" }}>{bus.remainStop} stop(s)</td>
              <td
                style={{
                  textAlign: "right",
                  paddingRight: "12px",
                  fontWeight: "bold",
                }}
              >
                {bus.remainTime <= 2
                  ? "DUE"
                  : `${Math.round(bus.remainTime / 60)} min`}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default App;
