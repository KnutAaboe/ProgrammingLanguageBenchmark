build-csharp:
	cd csharpfunctions2 && dotnet build

run-csharp:
	cd csharpfunctions2 && func host start

run-js:
	cd Nodejs && func host start

build-rust:
	find . -name Cargo.toml -depth 3 -execdir wasm-pack build --target nodejs \;
	rm -rf Rust/functions/node_modules && cd Rust/functions && npm i

run-rust:
	cd Rust/functions && func host start --javascript --debug