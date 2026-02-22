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

   if (type === "pie" || type === "doughnut") {
      delete options.scales;

      for (const ds of data.datasets ?? []) {
         ds.borderColor ??= "#000000";
      }
   } else {
      if (options?.scales?.y?.grid?.color == null) {
         options.scales ??= {};
         options.scales.y ??= {};
         options.scales.y.grid ??= {};
         options.scales.y.grid.color = getGridColor;
      }

      options.scales ??= {};
      options.scales.y ??= {};
      options.scales.y.border ??= {};
      options.scales.y.border.display = false;
   }

   return new Chart(ctx, {
      type: type,
      data: data,
      options: options,
   });
}

export function updateChart(chart, type, data, options) {
    chart.type = type;
    chart.data = data;
    chart.options = options;
}