document.addEventListener("DOMContentLoaded", () => {
    const emailInput = document.getElementById('emailInput');
    const studentFields = document.querySelectorAll('.student-field');
    const invigilatorFields = document.querySelectorAll('.invigilator-field');

    emailInput.addEventListener('input', function () {
        const value = emailInput.value.toLowerCase();

        if (value.endsWith("@busitema.ac.ug")) {
            studentFields.forEach(el => el.classList.remove('d-none'));
            invigilatorFields.forEach(el => el.classList.add('d-none'));
        } else if (value.endsWith("@staff.busitema.ac.ug")) {
            studentFields.forEach(el => el.classList.add('d-none'));
            invigilatorFields.forEach(el => el.classList.remove('d-none'));
        } else {
            studentFields.forEach(el => el.classList.add('d-none'));
            invigilatorFields.forEach(el => el.classList.add('d-none'));
        }
    });
});
