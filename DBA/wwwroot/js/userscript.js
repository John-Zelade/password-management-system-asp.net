var navLinks = document.querySelectorAll('.nav-link');

navLinks.forEach(function (link) {
    link.addEventListener('click', function () {

        navLinks.forEach(function (link) {
        link.classList.remove('active');
        });

this.classList.add('active');
  });
});

function loadPage(url) {

    $.ajax({
        url: url,
        method: 'GET',
        success: function (data) {

            $('#pageContent').html(data);
        },
        error: function () {
            alert('Error loading page.');
        }
    });
}
var userID = localStorage.getItem('UserId');

document.getElementById('usersID').value = userID;

