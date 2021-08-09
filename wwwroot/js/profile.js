$('#inline-sex').editable({
    url: '/Account/ProfileAc/?ac=gender',
    name: "gender",
    mode: 'inline',
    source: [{
        value: 0,
        text: 'ไม่ระบุ'
    },{
        value: 1,
        text: 'ชาย'
    }, {
        value: 2,
        text: 'หญิง'
    }],
    display: function(value, sourceData) {
        var colors = {
                0: "#98a6ad",
                1: "#5fbeaa",
                2: "#5d9cec"
            },
            elem = $.grep(sourceData, function(o) {
                return o.value == value;
            });

        if (elem.length) {
            $(this).text(elem[0].text).css("color", colors[value]);
        } else {
            $(this).empty();
        }
    },
    ajaxOptions: {
        dataType: 'json'
    },
    success: function(response, newValue) {
        if(!response) {
            return "Unknown error!";
        }

        if(response.success === false) {
             return response.msg;
        }
    }
});
$('#inline-fname').editable({
    url: '/Account/ProfileAc/?ac=fname',
    name: "fname",
    mode: 'inline',
    ajaxOptions: {
        dataType: 'json'
    },
    validate: function(value) {
        if($.trim(value) == '') return 'ต้องระบุ';
     },
    success: function(response, newValue) {
        if(!response) {
            return "Unknown error!";
        }
        if(response.success === false) {
             return response.msg;
        } else {
            $('#topmenucore_fname0').html(newValue);
        }

    }
});
$('#inline-lname').editable({
    url: '/Account/ProfileAc/?ac=lname',
    name: "lname",
    mode: 'inline',
    validate: function(value) {
        if($.trim(value) == '') return 'ต้องระบุ';
     },
    ajaxOptions: {
        dataType: 'json'
    },
    success: function(response, newValue) {
        if(!response) {
            return "Unknown error!";
        }
        if(response.success === false) {
             return response.msg;
        } else {
            $('#topmenucore_lname0').html(newValue);
        }
    }
});
var inlinedobdate = new Date();
$('#inline-dob').editable({
    url: '/Account/ProfileAc/?ac=dob',
    name: "dob",
    mode: 'inline',
    combodate: {
        minYear: inlinedobdate.getFullYear()-100+543,
        maxYear: inlinedobdate.getFullYear()-10+543,
    },
    validate: function(value) {
        if($.trim(value) == '' || value.length < 8) return 'กรุณาเลือกให้ครบ';
     },
    ajaxOptions: {
        dataType: 'json'
    },
    success: function(response, newValue) {
        if(!response) {
            return "Unknown error!";
        }
        if(response.success === false) {
             return response.msg;
        }
    }
});

$(function () {
    if(!$("#deleteavatar").hasClass("listened")){
        $("#deleteavatar").on('click', function () {
            Swal.fire({
                title: "แน่ใจหรือไม่?",
                text: "ท่านต้องการลบภาพโปรไฟล์ใช่หรือไม่!",
                type: "warning",
                showCancelButton: true,
                confirmButtonColor: "#DD6B55",
                confirmButtonText: "ใช่, ลบทิ้ง!",
                cancelButtonText: "ไม่ใช่, เก็บไว้",
                closeOnConfirm: true,
                closeOnCancel: true
            }).then((result) => {
                if (result.isConfirmed) {
                    var localurl = location.origin;
                    $.ajax(localurl+'/Account/ProfileAc/?ac=deleteavatar', {
                        method: 'POST',
                        processData: false,
                        contentType: false,
                        success: function (response) {
                            var outval;
                            var nextflag = false;
                            try {
                                outval = JSON.parse(response);
                                nextflag = true;
                            } catch(e) {
                                $alert.show().addClass('alert-danger').text(response);
                            }
                            if(nextflag){
                                if(outval.status == "ok"){
                                    $("#avatarshow").attr("src",outval.filename);
                                    $("#layoutcore-profileimg1").attr("src",outval.filename);
                                    $('#deleteavatar').hide();
                                } else {
                                    $alert.show().addClass('alert-danger').text(outval.text);
                                }
                            }
                        }
                    });
                }
            });
        });
    }

});

window.addEventListener('DOMContentLoaded', function () {
    var avatar = document.getElementById('uploadavatar');
    var image = document.getElementById('imageavatar');
    var input = document.getElementById('inputavatar');
    var $progress = $('.progress');
    var $progressBar = $('.progress-bar');
    var $alert = $('#alertavatar');
    var $modal = $('#modalavatar');
    var cropper;

    $('[data-toggle="tooltip"]').tooltip();

    input.addEventListener('change', function (e) {
      var files = e.target.files;
      var done = function (url) {
        input.value = '';
        image.src = url;
        $alert.hide();
        $modal.modal('show');
      };
      var reader;
      var file;
      var url;

      if (files && files.length > 0) {
        file = files[0];

        if (URL) {
          done(URL.createObjectURL(file));
        } else if (FileReader) {
          reader = new FileReader();
          reader.onload = function (e) {
            done(reader.result);
          };
          reader.readAsDataURL(file);
        }
      }
    });

    $modal.on('shown.bs.modal', function () {
      cropper = new Cropper(image, {
        aspectRatio: 1,
        viewMode: 2,
        zoomable: true,
        strict: true,
      });
    }).on('hidden.bs.modal', function () {
      cropper.destroy();
      cropper = null;
    });

    document.getElementById('crop').addEventListener('click', function () {
      var initialAvatarURL;
      var canvas;

      $modal.modal('hide');

      if (cropper) {
        canvas = cropper.getCroppedCanvas({
          width: 160,
          height: 160,
        });
        initialAvatarURL = avatar.src;
        avatar.src = canvas.toDataURL();
        $progress.show();
        $alert.removeClass('alert-success alert-warning');
        canvas.toBlob(function (blob) {
          var formData = new FormData();
          var localurl = location.origin;
          formData.append('avatar', blob, 'avatar.png');
          $.ajax(localurl+'/Account/ProfileAc/?ac=uploadavatar', {
            method: 'POST',
            data: formData,
            processData: false,
            contentType: false,

            xhr: function () {
              var xhr = new XMLHttpRequest();

              xhr.upload.onprogress = function (e) {
                var percent = '0';
                var percentage = '0%';

                if (e.lengthComputable) {
                  percent = Math.round((e.loaded / e.total) * 100);
                  percentage = percent + '%';
                  $progressBar.width(percentage).attr('aria-valuenow', percent).text(percentage);
                }
              };

              return xhr;
            },

            success: function (upres) {
                var nextflag = false;
                var outval;
                try {
                    outval = JSON.parse(upres);
                    nextflag = true;
                } catch(e) {
                    $alert.show().addClass('alert-danger').text(upres);
                }
                if(nextflag){
                    if(outval.status == "ok"){
                        $("#avatarshow").attr("src",outval.filename);
                        $("#layoutcore-profileimg1").attr("src",outval.filename);
                        $('#deleteavatar').show();
                    } else {
                        $alert.show().addClass('alert-danger').text(outval.text);
                    }
                }

            },

            error: function (upres) {
              avatar.src = initialAvatarURL;
              $alert.show().addClass('alert-warning').text('Upload error');
            },

            complete: function () {
              $progress.hide();
            },
          });
        });
      }
    });
  });

  $('#btnSubmit').click(function(event) {
    var form = $('#myForm');
    if (form[0].checkValidity() === false) {
      event.preventDefault();
      event.stopPropagation();
    }
    form.addClass('was-validated');
    if (form[0].checkValidity() === true) {
        event.preventDefault();
        event.stopPropagation();
        if($('#txt_password').val() == $('#txt_re_password').val()){
            $('#btnSubmit').hide();
            $('#txt_password').prop('readonly', true);
            $('#txt_re_password').prop('readonly', true);
            var localurl = location.origin;
            $.get(localurl+"/Account/ProfileAc/?ac=changepass", { password: $('#txt_password').val(), repassword: $('#txt_re_password').val() })
            .fail(function( data ) {
                $('#btnSubmit').show();
                $('#txt_password').prop('readonly', false);
                $('#txt_re_password').prop('readonly', false);
                swal({
                    title: data,
                });
            })
            .done(function( data ) {
                if(data == "ok"){
                    window.location.reload();
                } else {
                    $('#btnSubmit').show();
                    $('#txt_password').prop('readonly', false);
                    $('#txt_re_password').prop('readonly', false);
                    swal({
                        title: data,
                    });
                }
            });
        } else {
            swal({
                title: 'รหัสผ่านไม่ตรงกัน',
            });
        }
    }
    });