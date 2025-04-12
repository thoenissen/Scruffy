export function exportDivToImage(element, filename) {
   html2canvas(element, {
      backgroundColor: '#1A1A1A',
   }).then(canvas => {
      const link = document.createElement('a');
      link.download = filename;
      link.href = canvas.toDataURL("image/png");
      link.click();
   });
}