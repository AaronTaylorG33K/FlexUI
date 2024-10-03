export const getBackgroundColorClass = (color: string | undefined) => {
    switch (color) {
      case "blue":
        return "bg-purple-500 hover:bg-purple-600 text-white border border-purple-600/20";
      case "gray":
        return "bg-gray-500 hover:bg-gray-600 text-white border border-gray-600/20";
      // Add more colors as needed
      default:
        return "bg-gray-100 text-black hover:bg-gray-300 border border-white/20";
    }
  };