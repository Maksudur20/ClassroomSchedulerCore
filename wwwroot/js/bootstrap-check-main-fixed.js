// Script to verify Bootstrap is properly loaded
document.addEventListener('DOMContentLoaded', function() {
    // Check if Bootstrap JS is loaded
    if (typeof bootstrap === 'undefined') {
        console.error('Bootstrap JS is not loaded! Some functionality may be limited.');
        showBootstrapWarning();
    } else {
        console.log('Bootstrap JS is loaded successfully.');
    }
    
    // Check if Bootstrap CSS is loaded by testing a Bootstrap-specific property
    const testEl = document.createElement('div');
    testEl.className = 'dropdown-menu';
    document.body.appendChild(testEl);
    const computedStyle = window.getComputedStyle(testEl);
    
    if (computedStyle.position !== 'absolute' && computedStyle.display !== 'none') {
        console.error('Bootstrap CSS is not loaded correctly!');
        showBootstrapWarning();
    } else {
        console.log('Bootstrap CSS is loaded successfully.');
    }
    
    document.body.removeChild(testEl);
});

function showBootstrapWarning() {
    // Only show warning if element doesn't already exist
    if (!document.getElementById('bootstrap-warning')) {
        const alertDiv = document.createElement('div');
        alertDiv.id = 'bootstrap-warning';
        alertDiv.style.cssText = 'position:fixed;top:0;left:0;right:0;background-color:#dc3545;color:#fff;padding:10px;text-align:center;z-index:9999;font-family:sans-serif';
        alertDiv.innerHTML = 'Warning: Some styling components failed to load. Please try refreshing the page or contact your administrator.';
        
        const closeBtn = document.createElement('button');
        closeBtn.style.cssText = 'background:none;border:none;color:#fff;float:right;font-size:20px;cursor:pointer';
        closeBtn.innerHTML = '&times;';
        closeBtn.onclick = function() {
            document.body.removeChild(alertDiv);
        };
        
        alertDiv.appendChild(closeBtn);
        document.body.appendChild(alertDiv);
    }
}
