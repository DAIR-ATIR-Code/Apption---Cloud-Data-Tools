window.HelperFunctionInterop = {
    highlightJS: function (element) {
        console.log(element + "::Initializing highlight.js");
        
        $('pre code').each(function (i, block) {
            hljs.highlightBlock(block);
        });
        
        return true;
    },    
};