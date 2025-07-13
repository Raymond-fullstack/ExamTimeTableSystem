document.addEventListener('DOMContentLoaded', function() {
    // Sidebar navigation
    document.querySelectorAll('#sidebar .nav-link').forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            document.querySelectorAll('#sidebar .nav-link').forEach(l => l.classList.remove('active'));
            this.classList.add('active');
            loadSection(this.dataset.section);
        });
    });

    // Sidebar toggle for collapsibility
    const sidebarToggle = document.getElementById('sidebarToggle');
    const wrapper = document.getElementById('wrapper');
    if (sidebarToggle && wrapper) {
        sidebarToggle.addEventListener('click', function() {
            wrapper.classList.toggle('collapsed');
        });
    }

    // Initial load
    loadSection('departments');
});

function loadSection(section) {
    if (section === 'departments') {
        renderDepartments();
    } else {
        // Placeholder for other sections
        document.getElementById('main-content').innerHTML = `
            <div class="alert alert-info">Section "${section}" coming soon!</div>
        `;
    }
}

function renderDepartments() {
    const departments = [
        { id: 1, name: 'Computer Science', code: 'CS' },
        { id: 2, name: 'Mathematics', code: 'MATH' },
        { id: 3, name: 'Physics', code: 'PHY' }
    ];
    let html = `
    <div class="card shadow-sm mb-4">
        <div class="card-header d-flex justify-content-between align-items-center bg-info text-white">
            <h5 class="mb-0">Departments</h5>
            <button class="btn btn-success" id="addDepartmentBtn">Add Department</button>
        </div>
        <div class="card-body">
            <div class="mb-3">
                <input type="text" class="form-control" id="searchDepartment" placeholder="Search departments...">
            </div>
            <div class="table-responsive">
                <table class="table custom-table" id="departmentsTable">
                    <thead>
                        <tr>
                            <th data-sort="name">Name <span class="sort-icon"></span></th>
                            <th data-sort="code">Code <span class="sort-icon"></span></th>
                            <th>Actions</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${departments.map(d => `
                            <tr data-id="${d.id}">
                                <td>${d.name}</td>
                                <td>${d.code}</td>
                                <td>
                                    <button class="btn btn-info btn-sm edit-btn">Edit</button>
                                    <button class="btn btn-danger btn-sm delete-btn">Delete</button>
                                </td>
                            </tr>
                        `).join('')}
                    </tbody>
                </table>
            </div>
        </div>
    </div>
    `;
    document.getElementById('main-content').innerHTML = html;

    // Add event listeners
    document.getElementById('addDepartmentBtn').onclick = () => showDepartmentModal();
    document.querySelectorAll('.edit-btn').forEach(btn => {
        btn.onclick = function () {
            const row = this.closest('tr');
            showDepartmentModal({
                id: row.dataset.id,
                name: row.children[0].textContent,
                code: row.children[1].textContent
            });
        };
    });
    document.querySelectorAll('.delete-btn').forEach(btn => {
        btn.onclick = function () {
            const row = this.closest('tr');
            row.remove();
            showToast('Department deleted!', 'danger');
        };
    });
    document.getElementById('searchDepartment').oninput = function () {
        filterTable('departmentsTable', this.value);
    };
    // Table sorting
    document.querySelectorAll('#departmentsTable th[data-sort]').forEach(th => {
        th.onclick = function () {
            sortTable('departmentsTable', th.cellIndex);
        };
    });
}

function showDepartmentModal(dept = null) {
    document.getElementById('departmentModalLabel').textContent = dept ? 'Edit Department' : 'Add Department';
    document.getElementById('departmentId').value = dept ? dept.id : '';
    document.getElementById('departmentName').value = dept ? dept.name : '';
    document.getElementById('departmentCode').value = dept ? dept.code : '';
    const modal = new bootstrap.Modal(document.getElementById('departmentModal'));
    modal.show();

    document.getElementById('departmentForm').onsubmit = function (e) {
        e.preventDefault();
        // For demo, just show toast and close modal
        modal.hide();
        showToast(dept ? 'Department updated!' : 'Department added!', 'success');
        // In real app, update table via AJAX and re-render
    };
}

// Table filter
function filterTable(tableId, query) {
    const rows = document.querySelectorAll(`#${tableId} tbody tr`);
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        row.style.display = text.includes(query.toLowerCase()) ? '' : 'none';
    });
}

// Table sort
function sortTable(tableId, colIndex) {
    const table = document.getElementById(tableId);
    const tbody = table.tBodies[0];
    const rows = Array.from(tbody.rows);
    const asc = !table.dataset.sortAsc || table.dataset.sortAsc === 'false';
    rows.sort((a, b) => {
        const aText = a.cells[colIndex].textContent.trim();
        const bText = b.cells[colIndex].textContent.trim();
        return asc ? aText.localeCompare(bText) : bText.localeCompare(aText);
    });
    rows.forEach(row => tbody.appendChild(row));
    table.dataset.sortAsc = asc;
}

// Toast notification
function showToast(message, type = 'success') {
    const toastContainer = document.getElementById('toast-container');
    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-bg-${type} border-0 show mb-2`;
    toast.role = 'alert';
    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;
    toastContainer.appendChild(toast);
    setTimeout(() => toast.remove(), 4000);
}