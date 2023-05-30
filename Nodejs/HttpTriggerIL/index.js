function in_loop(n) {
    let x = 0, y = 0;

    for (var i = 0; i < n; i++) {
        x = 1 + 1;
        y = y + x;
    }
}

module.exports = async function (context, req) {
    context.log('JavaScript HTTP trigger function processed a request.');
    in_loop(10000000)
};