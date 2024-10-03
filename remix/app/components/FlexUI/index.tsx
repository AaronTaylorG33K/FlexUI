import { useState } from "react";
import { DragDropContext, DropResult } from "react-beautiful-dnd";
import { Page, Message, Mutation } from "../../types";
import { ComponentListItem } from "./ComponentListItem";
import { PageListItem } from "./PageListItem";
import { useWebSocket } from "../../util/websocket";


export default function FlexUI() {
  const [pages, setPages] = useState<Page[]>([]);
  const ws = useWebSocket(setPages);


  const onDragEnd = (result: DropResult) => {
    if (!result.destination) return;

    const { source, destination } = result;

    if (
      source.droppableId === destination.droppableId &&
      source.index === destination.index
    ) {
      return;
    }

    const sourcePageIndex = pages.findIndex((page) => page.id.toString() === source.droppableId);
    const destinationPageIndex = pages.findIndex((page) => page.id.toString() === destination.droppableId);

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

    // Update the page ordinal if theres a page move
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

    // Create a pageMove mutation if the source and destination pages are different
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
      const message: Message = {
        mutations,
      };
      ws.current.send(JSON.stringify(message));
    }
  };

  return (
    <>        
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
                    <ComponentListItem
                      key={component_id}
                      component={component}
                      componentPages={componentPages}
                    />
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
                <PageListItem key={page.id} page={page} />
              ))}
            </ul>
          </DragDropContext>
        </div>
      </div>
    </>
  );
}