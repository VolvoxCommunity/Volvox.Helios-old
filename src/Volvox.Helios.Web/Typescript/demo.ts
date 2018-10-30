
class demo {

    // Lets us define class wide variables and type
    firstName: string;
    lastName: string;

    // Constructors are used on new class
    constructor(firstName: string, lastName: string) {
        this.firstName = firstName;
        this.lastName = lastName;
    }

    // Method and requested value
    Output(append: string) {
        return "Hello " + this.firstName + " " + this.lastName + " " + append
    }
}

// Generate a new class
let d = new demo("james", "tombleson")

// Create the button object that we will insert into the page
let DemoButton = document.createElement('button');

// Define what is placed on the button
DemoButton.textContent = "Click me";

// Define what we will do when it is clicked
DemoButton.onclick = function() {
    alert(d.Output("new value"));
}

document.body.appendChild(DemoButton);