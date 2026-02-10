export function setupRowClickHandlers(gridContainer, dotnetHelper) {
    if (!gridContainer) {
        return;
    }

    const table = gridContainer.querySelector('table');

    if (!table) {
        return;
    }

    const tbody = table.querySelector('tbody');

    if (!tbody) {
        return;
    }

    tbody.addEventListener('click', (event) => {
        const row = event.target.closest('tr');

        if (!row) {
            return;
        }

        if (event.target.closest('a')) {
            return;
        }

        const firstCell = row.querySelector('td');

        if (!firstCell) {
            return;
        }

        const reportIdElement = firstCell.querySelector('[data-report-id]');

        if (!reportIdElement) {
            return;
        }

        const reportId = reportIdElement.dataset.reportId;

        if (!reportId || !dotnetHelper) {
            return;
        }

        dotnetHelper.invokeMethodAsync('SelectReportFromJs', reportId)
                    .catch(err => console.error('Error calling Blazor method:', err));
    });

    const rows = tbody.querySelectorAll('tr');

    rows.forEach(row => {
        row.style.cursor = 'pointer';
    });
}