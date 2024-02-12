var updatedRow;

function ShowSuccessMessage(message = "Saved successfully") {
    Swal.fire({
        icon: "success",
        title: "success",
        text: message,
        customClass: {
            confirmButton: "btn btn-primary"
        }
    });
}

function onModalSuccess(item) {
    ShowSuccessMessage();
    $('#Modal').modal('hide');
    if (updatedRow === undefined) {
        $('tbody').append(item);
    } else {
        $(updatedRow).replaceWith(item);
        updatedRow = undefined;
    }
    KTMenu.init();
    KTMenu.initHandlers();

}

function ShowErrorMessage(message = "Something went wrong!") {
    Swal.fire({
        icon: "error",
        title: "Oops...",
        text: message,
    });
}

    $(document).ready(function () {
        var message = $('#Message').text();
        if (message !== '') {
            ShowSuccessMessage();
        }

        $('body').delegate('.js-render-modal','click', function () {
            var btn = $(this);
            var modal = $('#Modal');
            modal.find('#ModalLabel').text(btn.data('title'));

            if (btn.data('update') !== undefined) {
                updatedRow = btn.parents('tr');

            }
            $.get({
                url: btn.data('url'),
                success: function (form) {
                    modal.find('.modal-body').html(form);
                    $.validator.unobtrusive.parse(modal);
                },
                error: function () {
                    ShowErrorMessage();
                }

            });
            modal.modal('show');
        });
    });