window.toasts = {
    success: function (msg) {
        M.toast({ html: msg, inDuration: 1000, classes: 'light-blue lighten-3 rounded' });
    },
    fail: function (msg) {
        M.toast({ html: msg, inDuration: 1000, classes: 'red rounded' });
    },
    warning: function (msg) {
        M.toast({ html: msg, inDuration: 1000, classes: 'yellow rounded' });
    }
};