import type { MetaFunction } from "@remix-run/node";
import FlexUI from "../components/FlexUI";

export const meta: MetaFunction = () => {
  return [
    { title: "New Remix App" },
    { name: "description", content: "Welcome to Remix!" },
  ];
};

export default function Index() {
  return (
    <>
      <div className="flex flex-col min-h-screen items-center justify-center ">
        <div className="w-full max-w-screen-lg py-12">
          <h1 className="text-4xl font-bold">Welcome to FlexUI!</h1>
          <div className="flex flex-col gap-4">
            <p className="text-lg">
              This fullstack app uses Remix, .NET 8, WebSockets and Postgres.
            </p>
            <p className="text-md">
              It lives on Kubernetes for scalability and performance.
            </p>
            <li className="text-xs font-thin">
              Drag and drop the page components between pages and re-order them.
            </li>
            <li className="text-xs font-thin">
              Open multiple browser tabs to see this action happen live for all
              users.
            </li>
          </div>
        </div>

        <FlexUI />
      </div>
    </>
  );
}
