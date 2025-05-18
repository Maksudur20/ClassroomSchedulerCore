// Site-wide JavaScript for Classroom Scheduler

// Bootstrap Component Initialization
document.addEventListener('DOMContentLoaded', function () {
    // Initialize Bootstrap components
    initializeBootstrapComponents();
    
    // Add any additional page initialization here
});

// Function to initialize all Bootstrap components
function initializeBootstrapComponents() {
    // Initialize tooltips
    if (typeof bootstrap !== 'undefined') {
        var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.forEach(function (tooltipTriggerEl) {
            new bootstrap.Tooltip(tooltipTriggerEl);
        });

        // Initialize popovers
        var popoverTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="popover"]'));
        popoverTriggerList.forEach(function (popoverTriggerEl) {
            new bootstrap.Popover(popoverTriggerEl);
        });
        
        // Initialize dropdowns
        var dropdownElementList = [].slice.call(document.querySelectorAll('.dropdown-toggle'));
        dropdownElementList.forEach(function (dropdownToggleEl) {
            new bootstrap.Dropdown(dropdownToggleEl);
        });
        
        // Initialize modals
        var modalElementList = [].slice.call(document.querySelectorAll('.modal'));
        modalElementList.forEach(function (modalEl) {
            new bootstrap.Modal(modalEl);
        });
        
        // Initialize toasts
        var toastElementList = [].slice.call(document.querySelectorAll('.toast'));
        toastElementList.forEach(function (toastEl) {
            new bootstrap.Toast(toastEl);
        });
    } else {
        console.warn('Bootstrap JS is not loaded. Some components may not function properly.');
    }
}

// Format date for display (YYYY-MM-DD)
function formatDate(date) {
    var d = new Date(date);
    var month = '' + (d.getMonth() + 1);
    var day = '' + d.getDate();
    var year = d.getFullYear();

    if (month.length < 2) month = '0' + month;
    if (day.length < 2) day = '0' + day;

    return [year, month, day].join('-');
}

// Format time for display (HH:MM)
function formatTime(date) {
    var d = new Date(date);
    var hours = '' + d.getHours();
    var minutes = '' + d.getMinutes();

    if (hours.length < 2) hours = '0' + hours;
    if (minutes.length < 2) minutes = '0' + minutes;

    return [hours, minutes].join(':');
}

// Get status color class based on booking status
function getStatusClass(status) {
    switch (status) {
        case 0: // Available
            return 'bg-success';
        case 1: // Reserved
            return 'bg-warning';
        case 2: // Emergency
            return 'bg-danger';
        default:
            return 'bg-secondary';
    }
}

// Confirm delete operations
function confirmDelete(message) {
    return confirm(message || 'Are you sure you want to delete this item?');
}

// Initialize calendar (for schedule view)
function initializeCalendar(elementId, events) {
    var calendarEl = document.getElementById(elementId);
    if (!calendarEl) return;

    var calendar = new FullCalendar.Calendar(calendarEl, {
        initialView: 'timeGridWeek',
        headerToolbar: {
            left: 'prev,next today',
            center: 'title',
            right: 'dayGridMonth,timeGridWeek,timeGridDay'
        },
        allDaySlot: false,
        slotMinTime: '07:00:00',
        slotMaxTime: '22:00:00',
        height: 'auto',
        events: events || [],
        eventTimeFormat: {
            hour: '2-digit',
            minute: '2-digit',
            hour12: false
        }
    });
    
    calendar.render();
    return calendar;
}
