import React from "react";
import { Droppable, Draggable } from "react-beautiful-dnd";
import { PageListItemProps } from "../../types";
import { getBackgroundColorClass } from "../../util";

export const PageListItem: React.FC<PageListItemProps> = ({ page }) => {
  return (
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
                    const backgroundColorClass = getBackgroundColorClass(component.settings.background_color);
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
  );
};
