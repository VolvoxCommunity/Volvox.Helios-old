var demo = /** @class */ (function () {
    // Constructors are used on new class
    function demo(firstName, lastName) {
        this.firstName = firstName;
        this.lastName = lastName;
    }
    // Method and requested value
    demo.prototype.Output = function (append) {
        return "Hello " + this.firstName + " " + this.lastName + " " + append;
    };
    return demo;
}());
// Generate a new class
var d = new demo("james", "tombleson");
// Create the button object that we will insert into the page
var DemoButton = document.createElement('button');
// Define what is placed on the button
DemoButton.textContent = "Click me";
// Define what we will do when it is clicked
DemoButton.onclick = function () {
    alert(d.Output("new value"));
};
document.body.appendChild(DemoButton);
//# sourceMappingURL=demo.js.map