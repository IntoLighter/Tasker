var passwordInput = $('#Password');
var passwordToggle = $('#password-toggle');
passwordToggle.on('click', function () {
    var type = passwordInput.attr('type') === 'password' ? 'text' : 'password';
    passwordInput.attr('type', type);
    passwordToggle.toggleClass('fa-eye-slash');
});
