/* =========================================================
   NEXUS AI CHATBOT
========================================================= */

document.addEventListener(
    "DOMContentLoaded",
    function () {

        "use strict";


        const launcher =
            document.getElementById(
                "nexusAiLauncher"
            );

        const chatWindow =
            document.getElementById(
                "nexusAiWindow"
            );

        const closeButton =
            document.getElementById(
                "nexusAiClose"
            );

        const minimizeButton =
            document.getElementById(
                "nexusAiMinimize"
            );

        const messages =
            document.getElementById(
                "nexusAiMessages"
            );

        const input =
            document.getElementById(
                "nexusAiInput"
            );

        const sendButton =
            document.getElementById(
                "nexusAiSend"
            );

        const typing =
            document.getElementById(
                "nexusAiTyping"
            );

        const suggestions =
            document.getElementById(
                "nexusAiSuggestions"
            );


        if (
            !launcher ||
            !chatWindow ||
            !messages ||
            !input ||
            !sendButton
        ) {
            return;
        }


        let waitingForResponse = false;


        /* =============================================
           OPEN / CLOSE
        ============================================= */

        function openChat() {

            chatWindow.classList.add("open");

            chatWindow.setAttribute(
                "aria-hidden",
                "false"
            );

            launcher.setAttribute(
                "aria-expanded",
                "true"
            );


            setTimeout(
                function () {
                    input.focus();
                },
                300
            );
        }


        function closeChat() {

            chatWindow.classList.remove("open");

            chatWindow.setAttribute(
                "aria-hidden",
                "true"
            );

            launcher.setAttribute(
                "aria-expanded",
                "false"
            );
        }


        launcher.addEventListener(
            "click",
            function () {

                if (
                    chatWindow.classList
                        .contains("open")
                ) {
                    closeChat();
                }
                else {
                    openChat();
                }
            }
        );


        if (closeButton) {

            closeButton.addEventListener(
                "click",
                closeChat
            );
        }


        if (minimizeButton) {

            minimizeButton.addEventListener(
                "click",
                closeChat
            );
        }


        /* =============================================
           TIME
        ============================================= */

        function getCurrentTime() {

            return new Intl.DateTimeFormat(
                undefined,
                {
                    hour: "numeric",
                    minute: "2-digit"
                }
            ).format(new Date());
        }


        /* =============================================
           CREATE MESSAGE
        ============================================= */

        function addMessage(
            message,
            sender
        ) {

            const wrapper =
                document.createElement("div");


            wrapper.className =
                sender === "user"
                    ? "nexus-ai-message nexus-ai-message-user"
                    : "nexus-ai-message nexus-ai-message-bot";


            if (sender === "bot") {

                const avatar =
                    document.createElement("div");

                avatar.className =
                    "nexus-ai-message-avatar";


                const icon =
                    document.createElement("i");

                icon.className =
                    "bi bi-stars";


                avatar.appendChild(icon);

                wrapper.appendChild(avatar);
            }


            const content =
                document.createElement("div");

            content.className =
                "nexus-ai-message-content";


            const bubble =
                document.createElement("div");

            bubble.className =
                "nexus-ai-bubble";


            /*
             * textContent prevents AI output from
             * injecting HTML into the website.
             */

            bubble.textContent = message;


            const time =
                document.createElement("span");

            time.className =
                "nexus-ai-message-time";

            time.textContent =
                getCurrentTime();


            content.appendChild(bubble);

            content.appendChild(time);

            wrapper.appendChild(content);

            messages.appendChild(wrapper);


            scrollToBottom();
        }


        /* =============================================
           SCROLL
        ============================================= */

        function scrollToBottom() {

            requestAnimationFrame(
                function () {

                    messages.scrollTop =
                        messages.scrollHeight;

                }
            );
        }


        /* =============================================
           TYPING INDICATOR
        ============================================= */

        function showTyping() {

            if (typing) {

                typing.classList.add(
                    "show"
                );
            }
        }


        function hideTyping() {

            if (typing) {

                typing.classList.remove(
                    "show"
                );
            }
        }


        /* =============================================
           TEXTAREA AUTO RESIZE
        ============================================= */

        function resizeInput() {

            input.style.height = "auto";

            input.style.height =
                Math.min(
                    input.scrollHeight,
                    100
                ) + "px";
        }


        input.addEventListener(
            "input",
            resizeInput
        );


        /* =============================================
           SEND MESSAGE
        ============================================= */

        async function sendMessage(
            customMessage = null
        ) {

            if (waitingForResponse) {
                return;
            }


            const message =
                (
                    customMessage ??
                    input.value
                ).trim();


            if (!message) {
                return;
            }


            if (message.length > 1000) {

                addMessage(
                    "Please keep your message under 1000 characters.",
                    "bot"
                );

                return;
            }


            addMessage(
                message,
                "user"
            );


            input.value = "";

            resizeInput();


            if (suggestions) {

                suggestions.style.display =
                    "none";
            }


            waitingForResponse = true;

            sendButton.disabled = true;

            showTyping();


            try {

                const response =
                    await fetch(
                        "/api/nexus-ai/chat",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body:
                                JSON.stringify({
                                    message: message
                                })
                        }
                    );


                let data = null;


                try {

                    data =
                        await response.json();

                }
                catch {

                    data = null;

                }


                hideTyping();


                if (
                    !response.ok ||
                    !data ||
                    !data.success
                ) {

                    addMessage(
                        data?.message ??
                        "I couldn't process that request. Please try again.",
                        "bot"
                    );

                    return;
                }


                addMessage(
                    data.message,
                    "bot"
                );

            }
            catch (error) {

                hideTyping();


                addMessage(
                    "I can't reach the NEXUS AI service right now. Please check your connection and try again.",
                    "bot"
                );

            }
            finally {

                waitingForResponse = false;

                sendButton.disabled = false;

                input.focus();
            }
        }


        /* =============================================
           SEND BUTTON
        ============================================= */

        sendButton.addEventListener(
            "click",
            function () {

                sendMessage();

            }
        );


        /* =============================================
           ENTER TO SEND
        ============================================= */

        input.addEventListener(
            "keydown",
            function (event) {

                /*
                 * Enter = send
                 * Shift + Enter = new line
                 */

                if (
                    event.key === "Enter" &&
                    !event.shiftKey
                ) {

                    event.preventDefault();

                    sendMessage();
                }
            }
        );


        /* =============================================
           QUICK QUESTIONS
        ============================================= */

        if (suggestions) {

            suggestions
                .querySelectorAll(
                    "[data-question]"
                )
                .forEach(
                    function (button) {

                        button.addEventListener(
                            "click",
                            function () {

                                const question =
                                    button.dataset.question;

                                if (question) {

                                    sendMessage(
                                        question
                                    );
                                }
                            }
                        );

                    }
                );
        }

    }
);