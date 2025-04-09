function getGridColor(context) {
   if (context.tick === undefined) {
      return "lightgrey";
   }

   if (context.tick.value === 0) {
      return "transparent";
   }
}

export function createChart(chartId, type, data, options) {
   const ctx = document.getElementById(chartId).getContext("2d");

   if (options?.scales?.y?.grid?.color == null) {
      options.scales ??= {};
      options.scales.y ??= {};
      options.scales.y.grid ??= {};
      options.scales.y.grid.color = getGridColor;
   }

   return new Chart(ctx, {
      type: type,
      data: data,
      options: options,
   });
}
