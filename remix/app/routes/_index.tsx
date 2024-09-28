import { useEffect, useState } from "react";
import type { MetaFunction } from "@remix-run/node";

export const meta: MetaFunction = () => {
  return [
    { title: "New Remix App" },
    { name: "description", content: "Welcome to Remix!" },
  ];
};

export default function Index() {
  const [messages, setMessages] = useState<string[]>([]);
  const [input, setInput] = useState("");

  useEffect(() => {
    const ws = new WebSocket("ws://localhost:5555/ws");

    ws.onopen = (event) => {
      console.log("open", event);
      console.log("Connected to WebSocket server");
      ws.send("Initial data request");
    };

    ws.onmessage = (event) => {
      console.log("Received message from WebSocket server", event.data);
      setMessages((prevMessages) => [...prevMessages, event.data]);
    };

    ws.onclose = () => {
      console.log("Disconnected from WebSocket server");
    };

    return () => {
      ws.close();
    };
  }, []);

  const sendMessage = () => {
    const ws = new WebSocket("ws://localhost:5555/ws");
    ws.onopen = () => {
      ws.send(input);
      setInput("");
    };
  };

  return (
    <div className="flex h-screen items-center justify-center">
      <div className="flex flex-col items-center gap-16">
        <h1 className="text-4xl font-bold">Welcome to Remix!</h1>
        <p className="text-lg">
          This is a new Remix app. You can start by editing the files in the
          <code className="px-2 py-1 bg-gray-100 text-gray-800 rounded-md">
            app/
          </code>
          directory.
        </p>
        <div>
          <input
            type="text"
            value={input}
            onChange={(e) => setInput(e.target.value)}
            className="border px-2 py-1"
          />
          <button onClick={sendMessage} className="ml-2 px-4 py-2 bg-blue-500 text-white rounded">
            Send
          </button>
        </div>
        <div>
          <h2 className="text-2xl font-bold">Messages</h2>
          <ul>
            {messages.map((message, index) => (
              <li key={index}>{message}</li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}