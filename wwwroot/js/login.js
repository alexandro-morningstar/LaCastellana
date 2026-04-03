/*********************************************************************************************************************
 *     █████╗ ██╗     ███████╗██╗  ██╗ █████╗ ███╗   ██╗██████╗ ██████╗  ██████╗ ███████╗    ███████╗███╗   ███╗     *
 *    ██╔══██╗██║     ██╔════╝╚██╗██╔╝██╔══██╗████╗  ██║██╔══██╗██╔══██╗██╔═══██╗██╔════╝    ██╔════╝████╗ ████║     *
 *    ███████║██║     █████╗   ╚███╔╝ ███████║██╔██╗ ██║██║  ██║██████╔╝██║   ██║███████╗    █████╗  ██╔████╔██║     *
 *    ██╔══██║██║     ██╔══╝   ██╔██╗ ██╔══██║██║╚██╗██║██║  ██║██╔══██╗██║   ██║╚════██║    ██╔══╝  ██║╚██╔╝██║     *
 *    ██║  ██║███████╗███████╗██╔╝ ██╗██║  ██║██║ ╚████║██████╔╝██║  ██║╚██████╔╝███████║    ███████╗██║ ╚═╝ ██║     *
 *    ╚═╝  ╚═╝╚══════╝╚══════╝╚═╝  ╚═╝╚═╝  ╚═╝╚═╝  ╚═══╝╚═════╝ ╚═╝  ╚═╝ ╚═════╝ ╚══════╝    ╚══════╝╚═╝     ╚═╝     *
 *                                                                                                                   *
 *                                                                                                                   *
 *                                 Copyright (c) 2025 Sinuhé Alejandro Gómez Hernández                               *
 *                                                                                                                   *
 *                              Permission is granted for free use, but NOT for sale/rent.                           *
 *                             Commercial use is prohibited without explicit authorization.                          *
 *                                                                                                                   *
 *********************************************************************************************************************/


function user_error_message(xhr) {
    let error_message = "Ooppss!! Ocurrió un error inesperado al realizar la consulta o al intentar recuperar la información.";

    switch(xhr.status) {
        case 400:
            error_message = "Ocurrió un error en la petición, inténtalo de nuevo";
            break;
        case 403:
            error_message = "No estás autorizado para ver este contenido o tu sesión expiró.";
            break;
        case 404:
            error_message = "Parece que el contenido que buscas no existe o fue eliminado.";
            break;
        case 405:
            error_message = "¿Qué intentas hacer? (-_-).";
            break;
        case 500:
            error_message = "Error interno de servidor.";
            break;
    };
    return error_message;
};


function login(username, password, csrf_token) {
    $('.container').css('background','#1A2228').html($('.spinnerWrapper').html()); //Activar el spinner de carga.

    let loginData = { //Esta estructura corresponde con el modelo en BackEnd para el inicio de sesión.
        Username: $.trim(username),
        Password: $.trim(password)
    };

    $.ajax({
        url: "/api/Auth/Login",
        type: "POST",
        contentType: "application/json",
        headers: {
            "RequestVerificationToken": csrf_token
        },
        data: JSON.stringify(loginData), //Se debe serializar el objeto.
        success: function(response, xhr) {
            if (response.success) { // True en servidor == Inicio de sesión exitoso.
                window.location.href = response.redirectUrl;
            } else if (!response.success) { // False en el servidor == Servidor no devolvió información (inicio de sesión fallido).
                Swal.fire({
                    title: "¡Oopss!",
                    text: `${response.message}`,
                    icon: "info",
                    confirmButtonText: "Entendido",
                    showCancelButton: false,
                    allowEscapeKey: false,
                    allowOutsideClick: false
                }).then((result) => {
                    if(result.isConfirmed) {
                        location.reload();
                    }
                })
            }
        },
        error: function(xhr) {
            if (xhr.status != 401) {
                message = user_error_message(xhr);
                estatus_code =  Number(xhr.status);
                window.location.href = `/Home/ErrorHandler?statusCode=${estatus_code}&customError=${message}`;
            } else { // Cuando si es 401 Forbidden (crendenciales inválidas), no mostrar como error.
                let server_message = xhr.responseJSON ? xhr.responseJSON.message : "Credenciales incorrectas.";
                Swal.fire({
                    title: "¡Oopss!",
                    text: `${server_message}`,
                    icon: "info",
                    confirmButtonText: "Entendido",
                    showCancelButton: false,
                    allowEscapeKey: false,
                    allowOutsideClick: false
                }).then((result) => {
                    if(result.isConfirmed) { location.reload(); }
                });
            };
        },
        complete: () => {
            console.log("Petición AJAX finalizada.");
        }
    });
};


function checkLength(input_data, min_length){
    input_data = $.trim(input_data);
    if(input_data.length < min_length){
        return true;
    } else {
        return false;
    }
};


var add_remove_effects = function(parentRef,classname_animation){
    var $a = parentRef.addClass(classname_animation);
    var $b = classname_animation;
    setTimeout(function(){
        $a.removeClass($b);
    },450);     
};

// --------------- Escuchar por evento Click -|
$('.nextBtn').on('click',function(){
    var parentRef =  $(this).parent(); // ------ Referencia al padre del elemento que detonó el evento -|
    var inputVal = parentRef.find('input').val(); // ------ Valor del input ubicado dentro del elemento padre referenciado -|

    if(checkLength(inputVal,1)){ // ------ Si no tiene contenido el input...
        add_remove_effects(parentRef,'shake'); // ...agrega/retira animación...
        $('.container').addClass('error'); // ...agregar clase error asociada al color rojo del cuerpo...
        setTimeout(() => {
            $('.container').removeClass('error');
        }, 5000); // ... retirar clase (color) -|
        return false;
    }

    if(!parentRef.hasClass('lastField')){ // ------ Si este no es el último elemento del form...

        parentRef.addClass('hide'); //...ocultar el actual...
        parentRef.next().addClass('shown').addClass('visible'); //...ir al siguiente elemento row y mostrarlo...
        $('.bullets span.active').removeClass('active').next().addClass('active'); //...desactivar la bullet actual y activar la siguiente...
        parentRef.next().find('input').focus(); //...habilitar escritura automáticamente en el siguiente input -|

    } else { // ------ Si es el último, submit (agregar AJAX) -|

        let username = $('#LoginUsername').val();
        let password = $('#LoginPassword').val();
        let csrf_token = $('input[name="__RequestVerificationToken"]').val();

        login(username, password, csrf_token); // Intentar iniciar sesión
    }

    $('.container').removeClass('error'); // ------ Si el usuario ingresa el campo antes de que termine el timer, cambia el color.
});

// --------------- Escuchar un Enter en el form -|
$('.row input').on('keypress', function(event) {

    if (event.which === 13) { // ------ "13" es el identificador de Enter -|
        event.preventDefault();
        $(this).closest('.row').find('.nextBtn').trigger('click'); // ------ Simular el click sobre el botón nextBtn -|
    };
})