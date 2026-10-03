/* ============================================================
   NEXUS PUBLIC WEBSITE
   PUBLIC.JS — CLEAN PREMIUM MOTION ENGINE
   Pure Vanilla JavaScript
============================================================ */

document.addEventListener("DOMContentLoaded", function () {

    "use strict";

    /* ========================================================
       SETTINGS
    ======================================================== */

    const reduceMotion = window.matchMedia(
        "(prefers-reduced-motion: reduce)"
    ).matches;

    const desktopMode = () => window.innerWidth > 991;


    /* ========================================================
       1. NAVBAR
    ======================================================== */

    function initNavbar() {

        const navbar = document.querySelector(".nexus-navbar");

        if (!navbar) {
            return;
        }

        let ticking = false;

        function updateNavbar() {

            navbar.classList.toggle(
                "scrolled",
                window.scrollY > 30
            );

            ticking = false;
        }

        function onScroll() {

            if (ticking) {
                return;
            }

            ticking = true;

            requestAnimationFrame(updateNavbar);
        }

        updateNavbar();

        window.addEventListener(
            "scroll",
            onScroll,
            { passive: true }
        );
    }


    /* ========================================================
       2. MOBILE NAVBAR AUTO CLOSE
    ======================================================== */

    function initMobileNavbar() {

        const navbarCollapse =
            document.querySelector(".navbar-collapse");

        if (!navbarCollapse) {
            return;
        }

        const navLinks =
            document.querySelectorAll(
                ".navbar-collapse .nav-link:not(.dropdown-toggle)"
            );

        navLinks.forEach(function (link) {

            link.addEventListener("click", function () {

                if (
                    window.innerWidth >= 992 ||
                    !navbarCollapse.classList.contains("show")
                ) {
                    return;
                }

                if (
                    typeof bootstrap === "undefined" ||
                    !bootstrap.Collapse
                ) {
                    return;
                }

                const collapse =
                    bootstrap.Collapse.getOrCreateInstance(
                        navbarCollapse
                    );

                collapse.hide();
            });
        });
    }


    /* ========================================================
       3. MOUSE AMBIENT GLOW
    ======================================================== */

    function initMouseGlow() {

        if (reduceMotion || !desktopMode()) {
            return;
        }

        if (document.querySelector(".nexus-mouse-glow")) {
            return;
        }

        const glow = document.createElement("div");

        glow.className = "nexus-mouse-glow";

        document.body.appendChild(glow);


        let targetX = window.innerWidth / 2;
        let targetY = window.innerHeight / 2;

        let currentX = targetX;
        let currentY = targetY;


        document.addEventListener(
            "mousemove",
            function (event) {

                targetX = event.clientX;
                targetY = event.clientY;

            },
            { passive: true }
        );


        function animateGlow() {

            currentX += (targetX - currentX) * 0.075;
            currentY += (targetY - currentY) * 0.075;

            glow.style.transform =
                `translate3d(
                    ${currentX - 260}px,
                    ${currentY - 260}px,
                    0
                )`;

            requestAnimationFrame(animateGlow);
        }

        animateGlow();
    }


    /* ========================================================
       4. SCROLL REVEAL ENGINE
    ======================================================== */

    function initScrollReveal() {

        const revealGroups = [

            {
                selector: ".section-heading",
                animation: ""
            },

            {
                selector: ".service-card",
                animation: ""
            },

            {
                selector: ".why-visual",
                animation: "nx-from-left"
            },

            {
                selector: ".feature-item",
                animation: "nx-from-right"
            },

            {
                selector: ".stat-box",
                animation: "nx-scale"
            },

            {
                selector: ".process-card",
                animation: ""
            },

            {
                selector: ".cta-box",
                animation: "nx-scale"
            },

            {
                selector:
                    ".about-image-wrapper, " +
                    ".about-content, " +
                    ".about-floating-card",
                animation: ""
            },

            {
                selector:
                    ".plan-card, " +
                    ".contact-card, " +
                    ".value-card",
                animation: ""
            },

            {
                selector:
                    ".footer-brand, " +
                    ".footer-column",
                animation: ""
            }

        ];


        revealGroups.forEach(function (group) {

            const elements =
                document.querySelectorAll(group.selector);

            elements.forEach(function (element, index) {

                /*
                 * Prevent accidental duplicate initialization
                 */

                if (element.dataset.nexusReveal === "true") {
                    return;
                }

                element.dataset.nexusReveal = "true";

                element.classList.add("nx-reveal");

                if (group.animation) {
                    element.classList.add(group.animation);
                }

                const delay =
                    Math.min(index % 4, 3) * 85;

                element.style.setProperty(
                    "--nx-delay",
                    `${delay}ms`
                );
            });
        });


        const revealElements =
            document.querySelectorAll(".nx-reveal");


        if (reduceMotion) {

            revealElements.forEach(function (element) {
                element.classList.add("nx-visible");
            });

            return;
        }


        const observer =
            new IntersectionObserver(
                function (entries, revealObserver) {

                    entries.forEach(function (entry) {

                        if (!entry.isIntersecting) {
                            return;
                        }

                        entry.target.classList.add(
                            "nx-visible"
                        );

                        revealObserver.unobserve(
                            entry.target
                        );
                    });

                },
                {
                    threshold: 0.10,
                    rootMargin: "0px 0px -30px 0px"
                }
            );


        revealElements.forEach(function (element) {

            observer.observe(element);

        });
    }


    /* ========================================================
       5. COUNTERS
    ======================================================== */

    function initCounters() {

        const counters =
            document.querySelectorAll(".counter");

        if (!counters.length) {
            return;
        }


        const statsSection =
            document.querySelector(".nexus-stats");


        function animateCounter(counter) {

            if (counter.dataset.counted === "true") {
                return;
            }

            counter.dataset.counted = "true";


            const target =
                Number(counter.dataset.target || 0);


            if (reduceMotion) {

                counter.textContent =
                    target.toLocaleString();

                return;
            }


            const duration = 1500;

            const startTime =
                performance.now();


            function update(currentTime) {

                const elapsed =
                    currentTime - startTime;

                const progress =
                    Math.min(
                        elapsed / duration,
                        1
                    );


                const eased =
                    1 - Math.pow(
                        1 - progress,
                        4
                    );


                const value =
                    Math.round(
                        target * eased
                    );


                counter.textContent =
                    value.toLocaleString();


                if (progress < 1) {

                    requestAnimationFrame(update);

                } else {

                    counter.textContent =
                        target.toLocaleString();

                }
            }


            requestAnimationFrame(update);
        }


        function startCounters() {

            counters.forEach(
                animateCounter
            );
        }


        if (!statsSection) {

            startCounters();

            return;
        }


        const observer =
            new IntersectionObserver(
                function (entries) {

                    if (!entries[0].isIntersecting) {
                        return;
                    }

                    startCounters();

                    observer.disconnect();

                },
                {
                    threshold: 0.25
                }
            );


        observer.observe(statsSection);
    }


    /* ========================================================
       6. PROCESS LINE
    ======================================================== */

    function initProcessAnimation() {

        const processWrapper =
            document.querySelector(".process-wrapper");

        if (!processWrapper) {
            return;
        }


        if (reduceMotion) {

            processWrapper.classList.add(
                "nx-process-active"
            );

            return;
        }


        const observer =
            new IntersectionObserver(
                function (entries) {

                    if (!entries[0].isIntersecting) {
                        return;
                    }

                    processWrapper.classList.add(
                        "nx-process-active"
                    );

                    observer.disconnect();

                },
                {
                    threshold: 0.25
                }
            );


        observer.observe(processWrapper);
    }


    /* ========================================================
       7. SERVICE CARD SPOTLIGHT + SOFT 3D TILT
    ======================================================== */

    function initServiceCards() {

        if (reduceMotion || !desktopMode()) {
            return;
        }


        const cards =
            document.querySelectorAll(".service-card");


        cards.forEach(function (card) {

            if (card.dataset.nexusTilt === "true") {
                return;
            }

            card.dataset.nexusTilt = "true";


            let frameId = null;


            card.addEventListener(
                "mousemove",
                function (event) {

                    if (frameId) {
                        cancelAnimationFrame(frameId);
                    }


                    frameId =
                        requestAnimationFrame(
                            function () {

                                const rect =
                                    card.getBoundingClientRect();


                                const mouseX =
                                    event.clientX - rect.left;

                                const mouseY =
                                    event.clientY - rect.top;


                                card.style.setProperty(
                                    "--mouse-x",
                                    `${mouseX}px`
                                );

                                card.style.setProperty(
                                    "--mouse-y",
                                    `${mouseY}px`
                                );


                                const rotateY =
                                    (
                                        mouseX /
                                        rect.width -
                                        0.5
                                    ) * 3.2;


                                const rotateX =
                                    (
                                        mouseY /
                                        rect.height -
                                        0.5
                                    ) * -3.2;


                                card.style.transform =
                                    `perspective(1100px)
                                     rotateX(${rotateX}deg)
                                     rotateY(${rotateY}deg)
                                     translateY(-6px)`;

                            }
                        );
                }
            );


            card.addEventListener(
                "mouseleave",
                function () {

                    if (frameId) {

                        cancelAnimationFrame(frameId);

                        frameId = null;
                    }


                    card.style.transform =
                        "perspective(1100px) " +
                        "rotateX(0deg) " +
                        "rotateY(0deg) " +
                        "translateY(0)";
                }
            );
        });
    }


    /* ========================================================
       8. WHY NEXUS PARALLAX
    ======================================================== */

    function initWhyParallax() {

        if (reduceMotion || !desktopMode()) {
            return;
        }


        const visual =
            document.querySelector(".why-visual");

        if (!visual) {
            return;
        }


        const image =
            visual.querySelector(".why-image");

        if (!image) {
            return;
        }


        let frameId = null;


        visual.addEventListener(
            "mousemove",
            function (event) {

                if (frameId) {
                    cancelAnimationFrame(frameId);
                }


                frameId =
                    requestAnimationFrame(
                        function () {

                            const rect =
                                visual.getBoundingClientRect();


                            const x =
                                (
                                    event.clientX -
                                    rect.left
                                ) /
                                rect.width -
                                0.5;


                            const y =
                                (
                                    event.clientY -
                                    rect.top
                                ) /
                                rect.height -
                                0.5;


                            image.style.setProperty(
                                "--nx-parallax-x",
                                `${x * 8}px`
                            );

                            image.style.setProperty(
                                "--nx-parallax-y",
                                `${y * 8}px`
                            );

                        }
                    );
            }
        );


        visual.addEventListener(
            "mouseleave",
            function () {

                if (frameId) {

                    cancelAnimationFrame(frameId);

                    frameId = null;
                }


                image.style.setProperty(
                    "--nx-parallax-x",
                    "0px"
                );

                image.style.setProperty(
                    "--nx-parallax-y",
                    "0px"
                );
            }
        );
    }


    /* ========================================================
       9. MAGNETIC BUTTONS — SUBTLE
    ======================================================== */

    function initMagneticButtons() {

        if (reduceMotion || !desktopMode()) {
            return;
        }


        const buttons =
            document.querySelectorAll(".btn-lg-nexus");


        buttons.forEach(function (button) {

            if (button.dataset.nexusMagnetic === "true") {
                return;
            }

            button.dataset.nexusMagnetic = "true";


            button.addEventListener(
                "mousemove",
                function (event) {

                    const rect =
                        button.getBoundingClientRect();


                    const x =
                        event.clientX -
                        rect.left -
                        rect.width / 2;


                    const y =
                        event.clientY -
                        rect.top -
                        rect.height / 2;


                    button.style.transform =
                        `translate3d(
                            ${x * 0.045}px,
                            ${y * 0.055 - 1}px,
                            0
                        )`;
                }
            );


            button.addEventListener(
                "mouseleave",
                function () {

                    button.style.transform = "";

                }
            );
        });
    }


    /* ========================================================
       10. SMOOTH INTERNAL LINKS
    ======================================================== */

    function initSmoothAnchors() {

        const links =
            document.querySelectorAll(
                'a[href^="#"]:not([href="#"])'
            );


        links.forEach(function (link) {

            link.addEventListener(
                "click",
                function (event) {

                    const targetId =
                        link.getAttribute("href");


                    if (!targetId) {
                        return;
                    }


                    let target = null;


                    try {

                        target =
                            document.querySelector(
                                targetId
                            );

                    } catch {

                        return;

                    }


                    if (!target) {
                        return;
                    }


                    event.preventDefault();


                    target.scrollIntoView({
                        behavior:
                            reduceMotion
                                ? "auto"
                                : "smooth",

                        block: "start"
                    });
                }
            );
        });
    }


    /* ========================================================
       11. PAGE READY
    ======================================================== */

    function initPageReady() {

        requestAnimationFrame(
            function () {

                document.body.classList.add(
                    "nexus-ready"
                );

            }
        );
    }


    /* ========================================================
       INITIALIZE
    ======================================================== */

    initPageReady();

    initNavbar();

    initMobileNavbar();

    initMouseGlow();

    initScrollReveal();

    initCounters();

    initProcessAnimation();

    initServiceCards();

    initWhyParallax();

    initMagneticButtons();

    initSmoothAnchors();

});