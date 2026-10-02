import { useState, useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import "./App.css";

interface BusArrivalData {
  stationId: number;
  stopName: string | null;
  routeNumber: string | null;
  destination: string | null;
  remainingTime: number;
  remainingStops: number;
}

function App() {
  const STATION_ID = 105; // Hardcoded default station
  const BACKEND_URL = "https://localhost:7113/bitHub"; // Update with your API URL

  const [connectionStatus, setConnectionStatus] =
    useState<string>("Disconnected");
  const [arrivals, setArrivals] = useState<BusArrivalData[]>([]);
  const [stationName, setStationName] = useState<string>(
    `Station ${STATION_ID}`,
  );

  const connectionRef = useRef<signalR.HubConnection | null>(null);

  useEffect(() => {
    // 2. Build the SignalR Connection with auto-reconnect
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(BACKEND_URL)
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    connectionRef.current = connection;

    // 3. Register Client-side listeners
    connection.on("ReceiveBusArrivals", (data: BusArrivalData[]) => {
      setArrivals(data);
      // Capture the stop name if it's available in the payload
      if (data.length > 0 && data[0].stopName) {
        setStationName(data[0].stopName);
      }
    });

    connection.onreconnecting((error) => {
      console.warn("SignalR reconnecting due to error:", error);
      setConnectionStatus("Reconnecting");
    });

    connection.onreconnected(async (connectionId) => {
      setConnectionStatus("Connected");
      console.log(`Reconnected with ID: ${connectionId}. Rejoining group...`);
      try {
        // Re-register to the station group after a server drop disconnect
        await connection.invoke("JoinStation", STATION_ID.toString());
      } catch (err) {
        console.error("Failed to rejoin group on reconnect:", err);
      }
    });

    connection.onclose((error) => {
      console.error("SignalR connection closed permanently:", error);
      setConnectionStatus("Disconnected");
    });

    // 4. Start Connection and Join Station Group
    async function startConnection() {
      try {
        await connection.start();
        setConnectionStatus("Connected");
        await connection.invoke("JoinStation", STATION_ID.toString());
        console.log(
          `Successfully connected and joined group for station: ${STATION_ID}`,
        );
      } catch (err) {
        console.error("SignalR Connection Error: ", err);
        setConnectionStatus("Connection Failed");
        // Retry connection setup after 5 seconds if backend is down on boot
        setTimeout(startConnection, 5000);
      }
    }

    startConnection();

    // Clean up connections when component unmounts
    return () => {
      if (connectionRef.current) {
        connectionRef.current.stop();
      }
    };
  }, []);

  return (
    <div
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
        <div style={{ display: "flex", alignItems: "center" }}>
          <span
            style={{
              display: "inline-block",
              width: "12px",
              height: "12px",
              borderRadius: "50%",
              backgroundColor:
                connectionStatus === "Connected"
                  ? "#4caf50"
                  : connectionStatus === "Reconnecting"
                    ? "#ffeb3b"
                    : "#f44336",
              marginRight: "8px",
            }}
          />
          <span>Status: {connectionStatus}</span>
        </div>
      </div>

      {/* Network Disruption Alert Banner */}
      {connectionStatus !== "Connected" && (
        <div
          style={{
            backgroundColor: "#d32f2f",
            color: "white",
            padding: "15px",
            textAlign: "center",
            margin: "20px 0",
            fontSize: "1.2rem",
            fontWeight: "bold",
          }}
        >
          ⚠️ Network connection lost. Attempting to restore updates
          automatically...
        </div>
      )}

      {/* Timetable Schedule Grid */}
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
          {arrivals.length === 0 ? (
            <tr>
              <td
                colSpan={4}
                style={{ textAlign: "center", padding: "40px", color: "#666" }}
              >
                No active arrivals scheduled. Waiting for live updates...
              </td>
            </tr>
          ) : (
            // Sort by remaining time ascending
            [...arrivals]
              .sort((a, b) => a.remainingTime - b.remainingTime)
              .map((bus, idx) => (
                <tr
                  key={idx}
                  style={{
                    borderBottom: "1px solid #222",
                    color: bus.remainingTime <= 3 ? "#ff9800" : "#fff",
                  }}
                >
                  <td style={{ padding: "16px", fontWeight: "bold" }}>
                    {bus.routeNumber}
                  </td>
                  <td>{bus.destination}</td>
                  <td style={{ textAlign: "right" }}>
                    {bus.remainingStops} stop(s)
                  </td>
                  <td
                    style={{
                      textAlign: "right",
                      paddingRight: "12px",
                      fontWeight: "bold",
                    }}
                  >
                    {bus.remainingTime <= 2
                      ? "DUE"
                      : `${bus.remainingTime} min`}
                  </td>
                </tr>
              ))
          )}
        </tbody>
      </table>
    </div>
  );
}

export default App;
