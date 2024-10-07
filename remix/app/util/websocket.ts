// websocket.ts
import { Dispatch, SetStateAction, useRef, useEffect } from 'react';
import { Page, Message } from '../types';

export const useWebSocket = (setPages: Dispatch<SetStateAction<Page[]>>) => {
  const ws = useRef<WebSocket | null>(null);

  useEffect(() => {
    // change this port to 5555 for local dev mode, 8080 for builds
    ws.current = new WebSocket("ws://localhost:5000/ws");

    ws.current.onopen = () => {
      console.log("Connected to WebSocket server");
      const message: Message = {
        mutations: [],
      };
      ws.current?.send(JSON.stringify(message));
    };

    ws.current.onmessage = (event) => {
      const data = event.data;
      const p = JSON.parse(data);
      console.log("Received message from WebSocket server", p);
      setPages(p.data.pages);
    };

    ws.current.onclose = () => {
      console.log("Disconnected from WebSocket server");
    };
    
    ws.current.onerror = (error) => {
      console.error("WebSocket error:", error);
    }

    return () => {
      ws.current?.close();
    };
  }, [setPages]);

  return ws;
};