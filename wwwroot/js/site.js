// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

new DataTable('#MyTable');


const fechaFinal = document.getElementById("fechaFinal");

const today = new Date().toISOString().split('T')[0];
fechaInitial.setAttribute("min", today);

fechaInitial.addEventListener("change", function () {
    fechaFinal.value = '';
    fechaFinal.setAttribute("min", fechaInitial.value);
});