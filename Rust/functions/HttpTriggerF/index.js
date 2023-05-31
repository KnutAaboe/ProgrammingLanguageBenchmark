let rusty = require("fasta-rust");

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
	console.log(rusty);
	console.log(rusty.__wasm);
    context.res = {
        body: rusty.main_js()
    };
};