import { useState, useEffect, useRef } from "react";
import * as signalR from "@microsoft/signalr";
import { useParams } from "react-router-dom";
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
  const { stationId } = useParams<{ stationId: string }>();
  const TARGET_STATION = stationId || "3399003"; // Hardcoded default station
  const BACKEND_URL = "http://192.168.17.220:5191/bitHub"; // Update with your API URL
  const RUST_WS_URL = "ws://127.0.0.1:3000/ws";

  const [connectionStatus, setConnectionStatus] =
    useState<string>("Disconnected");
  const [arrivals, setArrivals] = useState<StationData>();
  const [stationName, setStationName] = useState<string>(
    `Station ${TARGET_STATION}`,
  );
  const [battery, setBattery] = useState<number | null>(null);

  const connectionRef = useRef<signalR.HubConnection | null>(null);
  const rustWsRef = useRef<WebSocket | null>(null);
  const displayRef = useRef<HTMLDivElement | null>(null);
  const [currentTime, setCurrentTime] = useState<string>("");

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
  const formatKoreanDateTime = () => {
    const now = new Date();
    
    const year = now.getFullYear();
    // Pad months and days with a leading zero if single digit
    const month = String(now.getMonth() + 1).padStart(2, '0');
    const day = String(now.getDate()).padStart(2, '0');
    
    let hours = now.getHours();
    const minutes = String(now.getMinutes()).padStart(2, '0');
    
    // Determine Korean meridiem indicator (오전 / 오후)
    const ampm = hours >= 12 ? "오후" : "오전";
    
    // Convert 24h clock to standard 12h display formats
    hours = hours % 12;
    hours = hours ? hours : 12; // The hour '0' should be '12'
    const formattedHours = String(hours).padStart(2, '0');

    return `${year}년 ${month}월 ${day}일 ${ampm} ${formattedHours}:${minutes}`;
  };

  // Set the initial date and time immediately on layout mount
  setCurrentTime(formatKoreanDateTime());

  // Setup interval loop tracking exactly at the top of every minute frame transition
  const interval = setInterval(() => {
    setCurrentTime(formatKoreanDateTime());
  }, 60000);

  return () => clearInterval(interval);
}, []);

  useEffect(() => {
    if (!connectionRef.current) {
      connectionRef.current = new signalR.HubConnectionBuilder()
        .withUrl(BACKEND_URL)
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .build();
    }

    const connection = connectionRef.current;

    connection.on("ReceiveBusArrivals", (data: StationData) => {
      setArrivals(data);
      console.log(data)
      if (data.busList.length > 0 && data.stationName)
        setStationName(data.stationName);
    });

     // 1. Triggered IMMEDIATELY when the server goes offline or network drops
  connection.onreconnecting((error) => {
    console.warn("SignalR connection lost. Attempting automatic reconnection:", error);
    setConnectionStatus("Reconnecting / Server Offline");
  });

  // 2. Triggered when a dropped connection successfully hooks back up
    connection.onreconnected(async () => {
      setConnectionStatus("Connected");
      await connection.invoke("JoinStation", TARGET_STATION);
    });

    // 3. Triggered if all automatic retry attempts in your array fail completely
  connection.onclose((error) => {
    console.error("SignalR connection died permanently:", error);
    setConnectionStatus("Disconnected");
    
    // Optional fallback: Manually trigger a fresh restart cycle after your 30s array fails
    setTimeout(startConnection, 5000); 
  });


    async function startConnection() {
      try {
        if (connection.state !== signalR.HubConnectionState.Disconnected) {
          return;
        }

        await connection.start();
        setConnectionStatus("Connected");
        await connection.invoke("JoinStation", TARGET_STATION);
      } catch {
        setConnectionStatus("Connection Failed");
        setTimeout(startConnection, 5000);
      }
    }

    startConnection();
    return () => {

      connection.off("ReceiveBusArrivals");
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
  }, [TARGET_STATION]);

   // Utility to handle localized ETA strings matching the image patterns
  const formatEta = (remainTime: number) => {
    const mins = Math.round(remainTime / 60);
    if (mins <= 2) return <span className="eta-due">잠시후</span>;
    return <span className="eta-time">{mins}분</span>;
  };

  // Renders the precise step track visualization blocks based on remaining stops
  const renderProgressTrack = (remainStop: number, currentStation: string | null) => {
    const totalSegments:any = [1, 2, 3, 4 ,5];
    
    return (
      <div className="track-container">
        <div className="track-visual">
          {totalSegments.map((segmentIndex:any) => {
            // Highlighting active index block corresponding to inverted step marker
            const isBusHere = remainStop === segmentIndex || (segmentIndex === 5 && remainStop > 5)
            const isIncomingEdge = segmentIndex === 5 && remainStop > 5;

            return (
              <div 
                key={segmentIndex} 
                className={`track-segment ${isBusHere ? 'bus-here' : ''} ${isIncomingEdge ? 'bus-incoming' : ''}`}
              >
                {isBusHere ? segmentIndex : (segmentIndex === 4 && remainStop > 5 ? '**' : (segmentIndex === 5 && remainStop > 5 ? remainStop : segmentIndex))}
                {isIncomingEdge && <span className="track-bus-icon">🚌</span>}
              </div>
            );
          })}
        </div>
        <div className="track-current-location">
          현재: {currentStation || "정보 없음"}
        </div>
      </div>
    );
  };

  // return (
  //   <div
  //     ref={displayRef}
  //     style={{
  //       padding: "20px",
  //       fontFamily: "monospace",
  //       backgroundColor: "#121212",
  //       color: "#fff",
  //       minHeight: "100vh",
  //     }}
  //   >
  //     {/* Top Banner Status Bar */}
  //     <div
  //       style={{
  //         display: "flex",
  //         justifyContent: "space-between",
  //         borderBottom: "2px solid #333",
  //         paddingBottom: "10px",
  //       }}
  //     >
  //       <h2>DEPARTURE BOARD: {stationName.toUpperCase()}</h2>
  //       <div style={{ display: "flex", gap: "20px" }}>
  //         {battery !== null && <span>⚡ Battery: {battery}%</span>}
  //         <span>SignalR: {connectionStatus}</span>
  //       </div>
  //     </div>

  //     <table
  //       style={{
  //         width: "100%",
  //         textAlign: "left",
  //         marginTop: "20px",
  //         fontSize: "1.4rem",
  //         borderCollapse: "collapse",
  //       }}
  //     >
  //       <thead>
  //         <tr style={{ color: "#aaa", borderBottom: "1px solid #444" }}>
  //           <th style={{ padding: "12px" }}>ROUTE</th>
  //           <th>DESTINATION</th>
  //           <th style={{ textAlign: "right" }}>STOPS AWAY</th>
  //           <th style={{ textAlign: "right", paddingRight: "12px" }}>
  //             ARRIVING
  //           </th>
  //         </tr>
  //       </thead>
  //       <tbody>
  //         {arrivals?.busList.map((bus, idx) => (
  //           <tr
  //             key={idx}
  //             style={{
  //               borderBottom: "1px solid #222",
  //               color: bus.remainTime <= 3 ? "#ff9800" : "#fff",
  //             }}
  //           >
  //             <td style={{ padding: "16px", fontWeight: "bold" }}>
  //               {bus.routeNo}
  //             </td>
  //             <td>{bus.routeDirection}</td>
  //             <td style={{ textAlign: "right" }}>{bus.remainStop} stop(s)</td>
  //             <td
  //               style={{
  //                 textAlign: "right",
  //                 paddingRight: "12px",
  //                 fontWeight: "bold",
  //               }}
  //             >
  //               {bus.remainTime <= 2
  //                 ? "DUE"
  //                 : `${Math.round(bus.remainTime / 60)} min`}
  //             </td>
  //           </tr>
  //         ))}
  //       </tbody>
  //     </table>
  //   </div>
  // );
   return (
    <div ref={displayRef} className="bit-container">
      {/* Kiosk Display Top Header info lines */}
      <div className="bit-header">
        <div className="bit-title-area">
          <span className="bit-city">공주 특례시</span>
          <span className="bit-city-en">GONGJU CITY</span>
          
        </div>
        <span className="bit-station-id">정류장 ID {TARGET_STATION}</span>
        <div className="bit-station-name">{stationName}</div>
      </div>

      <div className="bit-meta">
        <div>{currentTime}</div>
        <div style={{ display: "flex", gap: "10px", fontSize: "11px", color: "#666" }}>
          {battery !== null && <span>🔋 {battery}%</span>}
          <span>SignalR: {connectionStatus}</span>
        </div>
      </div>

      {/* Marquee ticker bar updates */}
      <div className="bit-alert-bar">
        잠시 후 도착 : 
      </div>

      {/* Structured Display Table Layout */}
      <table className="bit-table">
        <thead>
          <tr>
            <th style={{ textAlign: "center" }}>노선번호</th>
            <th style={{ textAlign: "center" }}>예정</th>
            <th style={{ textAlign: "center" }}>좌석</th>
            <th style={{ width: "650px" }}>현재 버스 위치</th>
          </tr>
        </thead>
        <tbody>
          {arrivals?.busList && arrivals.busList.length > 0 ? (
            arrivals.busList.map((bus, idx) => (
              <tr key={idx}>
                <td className="route-cell">
                  <div className="route-badge">일반</div>
                  <div className="route-number">{bus.routeNo}</div>
                  <div className="route-desc">{bus.routeDirection}</div>
                </td>
                <td>{formatEta(bus.remainTime)}</td>
                <td className="seat-status">여유</td>
                <td className="track-cell">
                  {renderProgressTrack(bus.remainStop, bus.currSttnName)}
                </td>
              </tr>
            ))
          ) : (
            <tr>
              <td colSpan={4} style={{ padding: "40px 0", color: "#666", fontWeight: "bold", fontSize: "60px" }}>
                현재 운행 중인 버스가 없습니다.
              </td>
            </tr>
          )}
        </tbody>
      </table>

      {/* Public information announcement footer panels matching design */}
      <div className="bit-footer">
        <div className="footer-ad">
          <div className="ad-title">공주시 정신건강복지센터 (☎ 041-852-1094)</div>
          <ul className="ad-list">
            <li>정신건강 상담 및 교육</li>
            <li>치료비, 심리상담 지원</li>
            <li>위기·자살 예방 상담</li>
          </ul>
        </div>
      </div>
    </div>
  );

}

export default App;
