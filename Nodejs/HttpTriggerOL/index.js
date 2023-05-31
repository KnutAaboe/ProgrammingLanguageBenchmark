function out_loop(n) {
    let x = 1 +1, y = 0;

    for (var i = 0; i < n; i++) {
        y = y + x;
    }
}

module.exports = async function (context, req) {
    out_loop(10000000);
};