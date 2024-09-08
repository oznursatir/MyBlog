
        

            class MyUploadAdapter {
                constructor(loader) {
                    this.loader = loader;
                }

                upload() {
                    return this.loader.file
                        .then(file => new Promise((resolve, reject) => {
                            const data = new FormData();
                            data.append('upload', file);

                            fetch('/BlogPost/UploadImage', {
                                method: 'POST',
                                body: data
                            })
                                .then(response => response.json())
                                .then(result => {
                                    if (result.url) {
                                        resolve({
                                            default: result.url
                                        });
                                    } else {
                                        reject(result.message);
                                    }
                                })
                                .catch(error => {
                                    reject(error.message);
                                });
                        }));
                }

                abort() {
                    // Bu kısımda isteği iptal etme işlemi gerçekleştirilebilir
                }
            }

            function MyUploadAdapterPlugin(editor) {
                editor.plugins.get('FileRepository').createUploadAdapter = (loader) => {
                    return new MyUploadAdapter(loader);
                };
            }

            ClassicEditor
                .create(document.querySelector('#Content'), {
                    extraPlugins: [MyUploadAdapterPlugin]
                })
                .then(editor => {
                    window.editor = editor;
                })
                .catch(error => {
                    console.error(error);
                });

            document.getElementById('createForm').addEventListener('submit', function (event) {
                document.querySelector('#Content').value = window.editor.getData();
            });
        

    
