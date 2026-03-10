document.addEventListener('DOMContentLoaded', function () {
    const chatEl = document.getElementById('chat');
    const inputEl = document.getElementById('message');
    const sendBtn = document.getElementById('sendBtn');

    function appendMessage(sender, text, className) {
        const wrapper = document.createElement('div');
        wrapper.className = 'chat-message ' + (className || '');

        const avatar = document.createElement('div');
        avatar.className = 'avatar';
        avatar.textContent = sender === 'You' ? 'Y' : 'AI';

        const bubble = document.createElement('div');
        bubble.className = 'bubble';

        // If AI message contains fenced code block, render specially
        if (sender === 'AI' && containsFencedCode(text)) {
            const codeContainer = renderCodeContainerFromText(text);
            bubble.appendChild(codeContainer);
            bubble.classList.add('code');
        } else {
            bubble.innerHTML = `<div class="text">${escapeHtml(text)}</div>`;
        }

        wrapper.appendChild(avatar);
        wrapper.appendChild(bubble);
        chatEl.appendChild(wrapper);
        chatEl.scrollTop = chatEl.scrollHeight;
    }

    function containsFencedCode(text) {
        return /```([a-zA-Z0-9]*)\n[\s\S]*?```/.test(text);
    }

    function renderCodeContainerFromText(text) {
        // find first fenced block
        const match = text.match(/```([a-zA-Z0-9]*)\n([\s\S]*?)```/);
        const lang = (match && match[1]) ? match[1] : 'javascript';
        const code = match ? match[2] : text;

        const container = document.createElement('div');
        container.className = 'code-container';

        const toolbar = document.createElement('div');
        toolbar.className = 'code-toolbar';

        const btnCopy = document.createElement('button');
        btnCopy.title = 'Copy';
        btnCopy.innerHTML = '??';
        btnCopy.addEventListener('click', async () => {
            try { await navigator.clipboard.writeText(code); btnCopy.innerHTML = '?'; setTimeout(()=>btnCopy.innerHTML='??',900); } catch { btnCopy.innerHTML='?'; setTimeout(()=>btnCopy.innerHTML='??',900); }
        });

        const btnFormat = document.createElement('button');
        btnFormat.title = 'Format';
        btnFormat.innerHTML = '?';
        btnFormat.addEventListener('click', () => {
            let formatted = code;
            try {
                if (lang === 'html' || lang === 'xml' || lang === 'markup') formatted = html_beautify(code, { indent_size: 2 });
                else if (lang === 'css' || lang === 'scss') formatted = css_beautify(code, { indent_size: 2 });
                else formatted = js_beautify(code, { indent_size: 2 });

                // replace code block content
                pre.textContent = formatted;
                Prism.highlightElement(pre);
            } catch (e) {
                console.warn('format failed', e);
            }
        });

        const btnRun = document.createElement('button');
        btnRun.title = 'Run';
        btnRun.innerHTML = '?';
        btnRun.addEventListener('click', () => {
            // only run JS in an isolated iframe
            if (lang !== 'javascript' && lang !== 'js') return;
            const iframe = document.createElement('iframe');
            iframe.style.width = '100%';
            iframe.style.height = '160px';
            iframe.style.border = '1px solid rgba(255,255,255,0.04)';
            container.appendChild(iframe);
            const doc = iframe.contentWindow.document;
            doc.open();
            doc.write('<!doctype html><html><body><script>' + code + '<\/script></body></html>');
            doc.close();
        });

        toolbar.appendChild(btnCopy);
        toolbar.appendChild(btnFormat);
        toolbar.appendChild(btnRun);

        const codeBlock = document.createElement('div');
        codeBlock.className = 'code-block';

        const pre = document.createElement('pre');
        pre.className = 'language-' + (lang || 'javascript');
        pre.textContent = code;

        codeBlock.appendChild(pre);

        container.appendChild(toolbar);
        container.appendChild(codeBlock);

        // highlight
        Prism.highlightElement(pre);

        return container;
    }

    function escapeHtml(unsafe) {
        return unsafe
            .replaceAll('&', '&amp;')
            .replaceAll('<', '&lt;')
            .replaceAll('>', '&gt;')
            .replaceAll('"', '&quot;')
            .replaceAll("'", '&#039;');
    }

    async function send() {
        const msg = inputEl.value.trim();
        if (!msg) return;
        appendMessage('You', msg, 'you');
        inputEl.value = '';
        sendBtn.disabled = true;

        try {
            const res = await fetch('/api/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ message: msg })
            });

            if (!res.ok) {
                const txt = await res.text();
                appendMessage('AI', 'Error: ' + txt, 'error');
            } else {
                const data = await res.text();
                appendMessage('AI', data, 'ai');
            }
        } catch (err) {
            appendMessage('AI', 'Network error: ' + err.message, 'error');
        } finally {
            sendBtn.disabled = false;
        }
    }

    sendBtn.addEventListener('click', send);
    inputEl.addEventListener('keydown', function (e) { if (e.key === 'Enter') send(); });

    // expose for inline use (optional)
    window.chatSend = send;
});
