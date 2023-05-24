let rusty = require("inout-loop-rust");

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    if (req.query.name || (req.body && req.body.name)) {
        context.res = {
            // status: 200, /* Defaults to 200 */
            body: rusty.in_loop()
        };
    }
    else {
        context.res = {
            status: 400,
            body: "Function executed successfully ."
        };
    }  
};