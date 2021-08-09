$(function () {
    $('input[name=vusername]').keypress(function(event){
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if(keycode == '13'){
            $('input[name=vpassword]').focus();
        }
    });
    $('input[name=vpassword]').keypress(function(event){
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if(keycode == '13'){
            $('#submitBtn').click();
        }
    });
    $('#submitBtn').click(function (e) {
        var flag = true;
        e.preventDefault();
        if(!$('#errorTxt').hasClass('d-non')){
            $('#errorTxt').addClass("d-none");
        }
        if($('input[name=vusername]').val().length < 3){
            flag = false;
            $('input[name=vusername]').addClass('is-invalid');
            $('input[name=vusername]').focus();
        }
        if($('input[name=vpassword]').val().length < 4 && flag){
            flag = false;
            $('input[name=vpassword]').addClass('is-invalid');
            $('input[name=vpassword]').focus();
        }
        if(flag){
            $('#onloading').removeClass('d-none');
            $('#loginbox').addClass("d-none");
            setTimeout( function(){
                $.post("/signin_ac", { vusername: $('input[name=vusername]').val(),vpassword: $('input[name=vpassword]').val() }
                ).done(function(data) {
                    if(data == "ok"){
                        window.location.reload();
                    } else {
                        $('#onloading').addClass('d-none');
                        $('#loginbox').fadeIn('slow').removeClass('d-none');
                        $('#errorTxt').html(data);
                        $('#errorTxt').fadeIn('slow').removeClass('d-none');
                        $('input[name=vpassword]').val("");
                        $('input[name=vpassword]').focus();
                    }
                }).fail(function(data) {
                    $('#onloading').addClass('d-none');
                    $('#loginbox').fadeIn('slow').removeClass('d-none');
                    $('#errorTxt').html("Error:["+data.status + "] "+data.statusText);
                    $('#errorTxt').fadeIn('slow').removeClass('d-none');
                });
            },1500);
        }
    });
    $('input[name=vusername]').on('change', function () {
        if($(this).val().length < 3){
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
    $('input[name=vpassword]').on('change', function () {
        if($(this).val().length < 4){
            $(this).addClass('is-invalid');
        } else {
            $(this).removeClass('is-invalid');
        }
    });
});