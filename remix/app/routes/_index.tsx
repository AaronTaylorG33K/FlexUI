import { useEffect, useState, useRef } from "react";
import type { MetaFunction } from "@remix-run/node";

// pages.ts
export interface Page {
  id: number;
  title: string;
  slug: string;
  content: string;
}

// components.ts
export interface Component {
  id: number;
  page_id: number;
  name: string;
  settings: Record<string, any>;
  ordinal: number;
}

export interface Mutation {
  dbfield: string;
  value: string | number;
  row_id: number;
}
export interface Message {
  data: {
    pages: Page[];
    components: Component[];
  };
  mutations?: Mutation[];
}

export const meta: MetaFunction = () => {
  return [
    { title: "New Remix App" },
    { name: "description", content: "Welcome to Remix!" },
  ];
};

export default function Index() {
  
  const [pages, setPages] = useState<Page[]>([]);
  const [components, setComponents] = useState<Component[]>([]);
  const ws = useRef<WebSocket | null>(null);

  useEffect(() => {
    ws.current = new WebSocket("ws://localhost:5555/ws");

    ws.current.onopen = () => {
      ws.current?.send("");
    };

    ws.current.onmessage = (event) => {
      const data = event.data;
      console.log("Received message from WebSocket server", { data });
      const p = JSON.parse(data);
      console.log({p})
      setPages(p.data.pages);
      setComponents(p.data.components);
    };

    ws.current.onclose = () => {
      console.log("Disconnected from WebSocket server");
    };

    return () => {
      ws.current?.close();
    };
  }, []);

  return (
    <div className="flex min-h-screen items-center justify-center ">
      <div className="flex flex-row items-start gap-1 mx-auto bg-gray-900">
        <div className="w-1/2 p-12">
          <h1 className="text-4xl font-bold">Welcome to FlexUI!</h1>
          <div className="flex flex-col gap-4">
          <p className="text-lg">
            This fullstack app uses Remix, .NET 8, WebSockets and
            Postgres.</p>
          <p className="text-md">It lives on Kubernetes for scalability and performance.
          </p>
          <li className="text-xs font-thin">Drag and drop the page components between pages and re-order them.</li> 
          <li className="text-xs font-thin">Open multiple browser tabs to see this action happen live for all users.</li>
          </div>
        </div>
        <div className="w-1/2 h-full flex items-center">
          <ul className="grid grid-cols-3 gap-4 h-ful m-4 w-full">
            {pages.map((page, index) => (
              <li key={index}>
                <div className="w-full min-h-32 aspect-3/4 border border-white rounded-lg">
                {page?.Title}
                </div>
              </li>
            ))}
          </ul>
        </div>
      </div>
    </div>
  );
}
