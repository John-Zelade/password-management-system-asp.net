$(document).ready(function () {

    $("#loginForm").submit(function (event) {

        event.preventDefault();


        var username = $("#Username").val();
        var password = $("#Password").val();


        $.post("/Home/Login", { Username: username, Password: password }, function (data) {
            console.log(data);

            if (data.status === "Success") {
                console.log(data.userId);
 
                localStorage.setItem("UserId", data.userId);
                localStorage.setItem("Username", data.username);


                // Retrieve userId from localStorage
                var userId = localStorage.getItem('UserId');

                // Redirect to /User/Employees with userId parameter
                window.location.href = "/User/Accounts?userId=" + userId;

            } else {
                alert("Login failed. Please check username and password.");
            }
        });
    });

});





