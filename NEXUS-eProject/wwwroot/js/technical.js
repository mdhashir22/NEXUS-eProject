document.addEventListener("DOMContentLoaded", function () {

    const toggleButton =
        document.getElementById("technicalSidebarToggle");

    if (!toggleButton) {
        return;
    }

    toggleButton.addEventListener("click", function () {

        document.body.classList.toggle(
            "technical-sidebar-collapsed"
        );

    });

});