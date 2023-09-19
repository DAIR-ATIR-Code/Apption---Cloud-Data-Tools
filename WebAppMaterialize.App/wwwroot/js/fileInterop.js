window.fileInterop = {
    fileName: undefined,
    filePath: undefined,

    initializeFileInput: function () {
        $('#fileupload').fileupload({

            start: function (e) {
                $('#startAnalyze').removeClass("orange darken-3");
                $('#startAnalyze').addClass("grey lighten-1");
                $('#fileUploadProgress .determinate').css('width', '0%');
                $('#fileUploadProgress').show();
            },

            change: function (e, data) {
                console.log('Upload file changes');
                console.log(data.files[0].name);
                $('#file-path').val(data.files[0].name);
                window.HelperFunctionInterop.fileName = data.files[0].name;
            },

            progress: function (e, data) {
                var progress = parseInt(data.loaded / data.total * 100, 10);
                $('#fileUploadProgress .determinate').css(
                    'width',
                    progress + '%'
                );
            },

            done: function (e, data) {
                console.log("success!");
                $('#startAnalyze').removeClass("grey lighten-1");
                $('#startAnalyze').addClass("orange darken-3");
                $('#fileUploadProgress').hide();
                window.fileInterop.filePath = data.result.filePath;
            }
        });
    },

    

    getFilePath: function () {
        return window.fileInterop.filePath;
    },

    getFileName: function () {
        return window.HelperFunctionInterop.fileName;
    }
};