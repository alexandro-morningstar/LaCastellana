function fill_table() {
    $('#users-table').DataTable({
        'serverSide': true,
        'processing': true,
        'ajax': {
            'url': '/Users/GetUsers',
            'type': 'POST',
            'headers': { 'RequestVerificationToken': $('input[name="__RequestVerificationToken"]').val() },
            'data': function(d) {}
        },
        'columns': [
            { 'data': 'username' },
            {
                'data': null,
                'render': function(data, type, row) {
                    let name = row.name;
                    let middlename = row.middlename;
                    let pat_surname = row.pat_surname;
                    let mat_surname = row.mat_surname;

                    const fullname = `${name} ${middlename || ''} ${pat_surname} ${mat_surname || ''}`;

                    return fullname;
                }
            },
            { 'data': 'accessLevel' },
            { 'data': 'is_active' },
            {
                'data': null,
                'render': function(data, type, row) {
                    const user_id = row.user_id;
                    const username = row.username;

                    return `
                        <div id="action-btns">
                            <a id="view-btn" href="/Users/Details?user_id=${user_id}">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-eye-fill" viewBox="0 0 16 16">
                                    <path d="M10.5 8a2.5 2.5 0 1 1-5 0 2.5 2.5 0 0 1 5 0"/>
                                    <path d="M0 8s3-5.5 8-5.5S16 8 16 8s-3 5.5-8 5.5S0 8 0 8m8 3.5a3.5 3.5 0 1 0 0-7 3.5 3.5 0 0 0 0 7"/>
                                </svg>
                            </a>
                            <a id="edit-btn" href="/User/Edit?user_id=${user_id}">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-pen-fill" viewBox="0 0 16 16">
                                    <path d="m13.498.795.149-.149a1.207 1.207 0 1 1 1.707 1.708l-.149.148a1.5 1.5 0 0 1-.059 2.059L4.854 14.854a.5.5 0 0 1-.233.131l-4 1a.5.5 0 0 1-.606-.606l1-4a.5.5 0 0 1 .131-.232l9.642-9.642a.5.5 0 0 0-.642.056L6.854 4.854a.5.5 0 1 1-.708-.708L9.44.854A1.5 1.5 0 0 1 11.5.796a1.5 1.5 0 0 1 1.998-.001"/>
                                </svg>
                            </a>
                            <button id="delete-btn" data-user="${user_id}|${username}">
                                <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" fill="currentColor" class="bi bi-trash3-fill" viewBox="0 0 16 16">
                                    <path d="M11 1.5v1h3.5a.5.5 0 0 1 0 1h-.538l-.853 10.66A2 2 0 0 1 11.115 16h-6.23a2 2 0 0 1-1.994-1.84L2.038 3.5H1.5a.5.5 0 0 1 0-1H5v-1A1.5 1.5 0 0 1 6.5 0h3A1.5 1.5 0 0 1 11 1.5m-5 0v1h4v-1a.5.5 0 0 0-.5-.5h-3a.5.5 0 0 0-.5.5M4.5 5.029l.5 8.5a.5.5 0 1 0 .998-.06l-.5-8.5a.5.5 0 1 0-.998.06m6.53-.528a.5.5 0 0 0-.528.47l-.5 8.5a.5.5 0 0 0 .998.058l.5-8.5a.5.5 0 0 0-.47-.528M8 4.5a.5.5 0 0 0-.5.5v8.5a.5.5 0 0 0 1 0V5a.5.5 0 0 0-.5-.5"/>
                                </svg>
                            </button>
                        </div>
                    `;
                }
            }
        ]
    })
}

document.addEventListener( 'DOMContentLoaded', fill_table );