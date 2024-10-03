import React from "react";
import { ComponentListItemProps } from "../../types";

export const ComponentListItem: React.FC<ComponentListItemProps> = ({ component, componentPages }) => {
  const backgroundColorClass = `bg-white text-black hover:bg-gray-100 border border-white/20 cursor-pointer`;

  return (
    <li
      key={component.component_id}
      className={`text-sm border border-white/20 p-2 flex flex-row items-center gap-2 ${backgroundColorClass}`}
    >
      {component.name}{" "}
      <span className="label text-xs bg-gray-500 text-white rounded-full px-1 py-.5 font-thin">
        Component
      </span>
      <div className="flex flex-wrap gap-1">
        {componentPages.map((title, index) => (
          <span
            key={index}
            className="label text-xs bg-purple-500 text-white rounded-full px-1 py-.5 font-thin"
          >
            {title}
          </span>
        ))}
      </div>
    </li>
  );
};
