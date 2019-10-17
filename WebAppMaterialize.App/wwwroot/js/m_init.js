window.m_init = {
    select: function (element) {
        console.log(element + "::Initializing Select");
        M.FormSelect.init(element);
        return true;
    },
    collapsible: function (element) {
        console.log(element + "::Initializing Collapsible");
        M.Collapsible.init(element);
        return true;
    },

    tabs: function (element) {
        console.log(element + "::Initializing tabs");
        M.Tabs.init(element);
        return true;
    },

    floatingActionButton: function (element) {
        console.log(element + "::Initializing floating point action");
        M.FloatingActionButton.init(element);
        var clipboard = new ClipboardJS('.btn');
        //initializing tootTip
        $('.tooltipped').each(function (i, element) {
            M.Tooltip.init(element);
        });
        return true;
    },
    
    modal: function (element) {
        M.Modal.init(element);
    }
};