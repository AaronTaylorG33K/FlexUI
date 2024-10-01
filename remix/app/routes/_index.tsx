import { useEffect, useState, useRef } from "react";
import type { MetaFunction } from "@remix-run/node";
import { DragDropContext, Droppable, Draggable } from "react-beautiful-dnd";

// pages.ts
export interface Page {
  id: number;
  title: string;
  slug: string;
  content: string;
  components: Component[];
}

// components.ts
export interface Component {
  component_id: number;
  id: number;
  name: string;
  settings: Record<string, any>;
  ordinal: number;
}

// mutations.ts
export interface Mutation {
  type: "ordinalUpdate" | "pageMove";
  newOrdinal?: number;
  destinationPageID?: number;
  pageComponentID: number;
  componentName?: string;
}

export interface Message {
  data?: {
    pages: Page[];
  };
  mutations?: Mutation[];
}

export const meta: MetaFunction = () => {
  return [
    { title: "New Remix App" },
    { name: "description", content: "Welcome to Remix!" },
  ];
};

const getBackgroundColorClass = (color: string | undefined) => {
  switch (color) {
    case "blue":
      return "bg-blue-500 hover:bg-blue-600 text-white border border-blue-600/20";
    case "gray":
      return "bg-gray-500 hover:bg-gray-600 text-white border border-gray-600/20";
    // Add more colors as needed
    default:
      return "bg-gray-100 text-black hover:bg-gray-300 border border-white/20";
  }
};

export default function Index() {
  const [pages, setPages] = useState<Page[]>([]);
  const ws = useRef<WebSocket | null>(null);

  useEffect(() => {
    ws.current = new WebSocket("ws://localhost:5555/ws");

    ws.current.onopen = () => {
      const message: Message = {
        mutations: [],
      };
      ws.current?.send(JSON.stringify(message));
    };

    ws.current.onmessage = (event) => {
      const data = event.data;
      const p = JSON.parse(data);
      console.log({ p });
      setPages(p.data.pages);
    };

    ws.current.onclose = () => {
      // console.log("Disconnected from WebSocket server");
    };

    return () => {
      ws.current?.close();
    };
  }, []);

  const onDragEnd = (result) => {
    if (!result.destination) return;

    const { source, destination } = result;

    if (
      source.droppableId === destination.droppableId &&
      source.index === destination.index
    ) {
      return;
    }

    const sourcePageIndex = pages.findIndex(
      (page) => page.id.toString() === source.droppableId
    );
    const destinationPageIndex = pages.findIndex(
      (page) => page.id.toString() === destination.droppableId
    );

    const sourcePage = pages[sourcePageIndex];
    const destinationPage = pages[destinationPageIndex];

    const [movedComponent] = sourcePage.components.splice(source.index, 1);

    // Check if the component already exists in the destination page
    const componentExists = destinationPage.components.some(
      (component) => component.component_id === movedComponent.component_id
    );

    if (!componentExists) {
      if (sourcePage.id === destinationPage.id) {
        sourcePage.components.splice(destination.index, 0, movedComponent);
      } else {
        destinationPage.components.splice(destination.index, 0, movedComponent);
      }
    } else {
      // If the component already exists, add it back to the source page at the original position
      sourcePage.components.splice(source.index, 0, movedComponent);
      return; // Exit early to avoid sending unnecessary updates
    }

    // Update the ordinal values and create mutations
    const mutations: Mutation[] = [];
    sourcePage.components.forEach((component, index) => {
      const newOrdinal = index + 1; // Start ordinal at 1
      if (component.ordinal !== newOrdinal) {
        mutations.push({
          type: "ordinalUpdate",
          newOrdinal: newOrdinal,
          pageComponentID: component.id,
          componentName: component.name,
        });
        component.ordinal = newOrdinal;
      }
    });
    destinationPage.components.forEach((component, index) => {
      const newOrdinal = index + 1; // Start ordinal at 1
      if (component.ordinal !== newOrdinal) {
        mutations.push({
          type: "ordinalUpdate",
          newOrdinal: newOrdinal,
          pageComponentID: component.id,
          componentName: component.name,
        });
        component.ordinal = newOrdinal;
      }
    });

    // Add mutation for page_id if the component was moved to a different page
    if (sourcePage.id !== destinationPage.id) {
      mutations.push({
        type: "pageMove",
        destinationPageID: destinationPage.id,
        pageComponentID: movedComponent.id,
        componentName: movedComponent.name,
      });
    }

    const newPages = [...pages];
    newPages[sourcePageIndex] = sourcePage;
    newPages[destinationPageIndex] = destinationPage;

    setPages(newPages);

    // Broadcast the mutations to the WebSocket server only if there are mutations
    if (mutations.length > 0 && ws.current?.readyState === WebSocket.OPEN) {
      console.log(
        "sending",
        JSON.stringify({ type: "updateOrder", mutations })
      );
      const message: Message = {
        mutations,
      };
      ws.current.send(JSON.stringify(message));
    }
  };

  return (
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
      <div className="flex flex-row w-full max-w-screen-lg items-start gap-1 mx-auto bg-gray-200/20">
        <div className="w-1/2 p-4">
          <h1 className="text-2xl">Components</h1>
          <p className="text-sm font-thin">
            Each of these is shared with a parent page. Conceptually you could
            duplicate these objects if you wanted unique content for the
            component on a specific page.
          </p>

          <div>
            <ul className="flex flex-col gap-2 mt-4">
              {Array.from(
                new Set(
                  pages.flatMap((page) =>
                    page.components.map((component) => component.component_id)
                  )
                )
              )
                .sort((a, b) => a - b) // Sort by component_id
                .map((component_id) => {
                  const component = pages
                    .flatMap((page) => page.components)
                    .find((c) => c.component_id === component_id);
                  const componentPages = pages
                    .filter((page) =>
                      page.components.some(
                        (c) => c.component_id === component_id
                      )
                    )
                    .map((page) => page.title);
                  return (
                    <li
                      key={component_id}
                      className="text-sm border border-white/20 p-2 flex flex-row items-center gap-2"
                    >
                      {component?.name}{" "}
                      <span className="label text-xs bg-gray-500 text-white rounded-full px-1 py-.5 font-thin">
                        Component
                      </span>
                      <div className="flex flex-wrap gap-1">
                        {componentPages.map((title, index) => (
                          <span
                            key={index}
                            className="label text-xs bg-blue-500 text-white rounded-full px-1 py-.5 font-thin"
                          >
                            {title}
                          </span>
                        ))}
                      </div>
                    </li>
                  );
                })}
            </ul>
          </div>
        </div>
        <div className="w-1/2 h-full flex flex-col items-center">
          <h1 className="text-2xl w-full mt-4">Pages</h1>

          <DragDropContext onDragEnd={onDragEnd}>
            <ul className="grid grid-cols-3 gap-4 h-full m-4 w-full pr-4">
              {pages.map((page) => (
                <Droppable droppableId={page.id.toString()} key={page.id}>
                  {(provided) => (
                    <li
                      ref={provided.innerRef}
                      {...provided.droppableProps}
                      className="w-full min-h-32 aspect-3/4"
                    >
                      <h2 className="text-sm select-none">{page.title}</h2>
                      <ul className="flex flex-col gap-1 py-3">
                        {page.components
                          .sort((a, b) => a.ordinal - b.ordinal)
                          .map((component, index) => (
                            <Draggable
                              key={`${page.id}-${component.id}`}
                              draggableId={`${page.id}-${component.id}`}
                              index={index}
                            >
                              {(provided) => {
                                const backgroundColorClass =
                                  getBackgroundColorClass(
                                    component?.settings.background_color
                                  );
                                return (
                                  <li
                                    ref={provided.innerRef}
                                    {...provided.draggableProps}
                                    {...provided.dragHandleProps}
                                    className={`text-xs p-1 font-thin cursor-pointer ${backgroundColorClass}`}
                                  >
                                    ⚙️ {component.name}
                                  </li>
                                );
                              }}
                            </Draggable>
                          ))}
                        {provided.placeholder}
                      </ul>
                    </li>
                  )}
                </Droppable>
              ))}
            </ul>
          </DragDropContext>
        </div>
      </div>
    </div>
  );
}
