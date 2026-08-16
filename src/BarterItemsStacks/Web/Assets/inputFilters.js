(function () {
    "use strict";

    const ATTACHED_ATTR = "data-filter-attached";
    const FILTER_ATTR = "data-filter";
    let _observer = null;

    function getKind(el) {
        return (el.getAttribute(FILTER_ATTR) || "").toLowerCase();
    }

    function sanitizeText(text, kind) {
        let s = (text ?? "").toString();

        if (kind === "uint") {
            s = s.replace(/[^\d]/g, "");

            if (s.length > 1 && s[0] === "0") {
                s = s.replace(/^0+/, "");
                if (s === "") s = "0";
            }
            return s;
        }

        if (kind === "udouble") {
            s = s.replace(/,/g, ".").replace(/[^\d.]/g, "");

            const firstDot = s.indexOf(".");
            if (firstDot !== -1) {
                s = s.slice(0, firstDot + 1) + s.slice(firstDot + 1).replace(/\./g, "");
            }

            if (s.startsWith(".")) s = "0" + s;
            
            if (s.length > 1 && s[0] === "0" && s[1] !== ".") {
                s = s.replace(/^0+/, "");
                if (s === "" || s.startsWith(".")) s = "0" + s;
            }

            return s;
        }

        if (kind === "weight") {
            s = s.replace(/,/g, ".").replace(/[^\d.]/g, "");

            const firstDot = s.indexOf(".");
            if (firstDot !== -1) {
                s = s.slice(0, firstDot + 1) + s.slice(firstDot + 1).replace(/\./g, "");
            }

            if (s.length > 1 && s[0] === "0" && s[1] !== ".") {
                s = s.replace(/^0+/, "");
                if (s === "" || s.startsWith(".")) s = "0" + s;
            }

            const hasDot = s.indexOf(".") !== -1;
            const parts = s.split(".");
            let intPart = parts[0] || "";
            let fracPart = parts.length > 1 ? parts[1] : "";

            if (intPart.length > 3) intPart = intPart.slice(0, 3);
            if (fracPart.length > 3) fracPart = fracPart.slice(0, 3);
            if (intPart.length + fracPart.length > 4) {
                fracPart = fracPart.slice(0, Math.max(0, 4 - intPart.length));
            }

            return hasDot ? intPart + "." + fracPart : intPart;
        }

        return s;
    }

    function getSelection(el) {
        const start = (typeof el.selectionStart === "number") ? el.selectionStart : el.value.length;
        const end = (typeof el.selectionEnd === "number") ? el.selectionEnd : el.value.length;
        return { start, end };
    }

    function valueAfterInsert(el, insertText) {
        const sel = getSelection(el);
        return el.value.slice(0, sel.start) + insertText + el.value.slice(sel.end);
    }

    function isAllowedInsertion(el, kind, data) {
        if (data == null) return true;

        const t = data.toString();

        if (kind === "uint") {
            if (!/^[0-9]*$/.test(t)) return false;
            
            const sel = getSelection(el);
            const next = valueAfterInsert(el, t);
            
            if (/^0\d/.test(next)) return false;

            return true;
        }

        if (kind === "udouble") {
            if (!/^[0-9.,]*$/.test(t)) return false;

            const next = valueAfterInsert(el, t).replace(/,/g, ".");
            const dotCount = (next.match(/\./g) || []).length;
            return dotCount <= 1;
        }

        if (kind === "weight") {
            if (!/^[0-9.,]*$/.test(t)) return false;

            const next = valueAfterInsert(el, t).replace(/,/g, ".");
            const dotCount = (next.match(/\./g) || []).length;
            if (dotCount > 1) return false;

            const parts = next.split(".");
            const intPart = parts[0] || "";
            const fracPart = parts.length > 1 ? parts[1] : "";

            if (intPart.length > 3) return false;
            if (fracPart.length > 3) return false;
            if (intPart.length + fracPart.length > 4) return false;

            return true;
        }

        return true;
    }

    function dispatchInput(el) {
        el.dispatchEvent(new Event("input", { bubbles: true }));
    }

    function setValueAndNotify(el, value) {
        el.value = value;
        dispatchInput(el);
    }

    function attachOne(el) {
        if (!(el instanceof HTMLInputElement)) return;

        const kind = getKind(el);
        if (kind !== "uint" && kind !== "udouble" && kind !== "weight") return;

        if (el.getAttribute(ATTACHED_ATTR) === "1") return;
        el.setAttribute(ATTACHED_ATTR, "1");

        el.addEventListener("beforeinput", (e) => {
            const type = e.inputType || "";
            const isInsert = type.startsWith("insert");
            if (!isInsert) return;

            if (!isAllowedInsertion(el, kind, e.data)) {
                e.preventDefault();
            }
        });

        function handleExternalInput(e) {
            e.preventDefault();
            const text = e.clipboardData?.getData("text") || e.dataTransfer?.getData("text") || "";
            const cleaned = sanitizeText(text, kind);
            const next = sanitizeText(valueAfterInsert(el, cleaned), kind);
            setValueAndNotify(el, next);
        }

        el.addEventListener("paste", handleExternalInput);
        el.addEventListener("drop", handleExternalInput);
        el.addEventListener("input", () => {
            const sel = getSelection(el);
            const cleaned = sanitizeText(el.value, kind);
            if (el.value !== cleaned) {
                const cursorShift = cleaned.length - el.value.length;
                el.value = cleaned;
                
                const newPos = Math.max(0, Math.min(sel.start + cursorShift, cleaned.length));
                el.setSelectionRange(newPos, newPos);

                dispatchInput(el);
            }
        });

        el.addEventListener("blur", () => {
            const shouldClear = el.hasAttribute("data-clear-on-blur") || 
                               (el.classList && el.classList.contains("summaryInput"));
            if (shouldClear) {
                el.value = "";
            }
        });
    }

    function attachWithin(root) {
        if (!root) return;

        if (root instanceof HTMLInputElement) {
            attachOne(root);
            return;
        }

        if (!root.querySelectorAll) return;

        root
            .querySelectorAll('input[data-filter="uint"], input[data-filter="udouble"], input[data-filter="weight"]')
            .forEach(attachOne);
    }

    function startObserver() {
        attachWithin(document);

        if (_observer) return;

        _observer = new MutationObserver((mutations) => {
            for (const m of mutations) {
                if (m.addedNodes && m.addedNodes.length) {
                    m.addedNodes.forEach((n) => attachWithin(n));
                }
                if (m.type === "attributes") {
                    attachWithin(m.target);
                }
            }
        });

        _observer.observe(document.body, {
            childList: true,
            subtree: true,
            attributes: true,
            attributeFilter: [FILTER_ATTR]
        });
    }

    function stopObserver() {
        if (_observer) {
            _observer.disconnect();
            _observer = null;
        }
    }

    window.inputFilters = {
        attachAll: function () { attachWithin(document); },
        startObserver: function () { startObserver(); },
        stopObserver: function () { stopObserver(); }
    };
})();