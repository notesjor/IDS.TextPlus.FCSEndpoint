# TextPlus OpenAPI Python Client

Python client for the CLARIN FCS Aggregator REST API.

## Installation

Install locally:

```bash
pip install .
```

Build wheel and source distribution:

```bash
python -m build
```

Publish to PyPI:

```bash
python -m twine upload dist/*
```

## Usage

```python
import openapi_client

configuration = openapi_client.Configuration()
configuration.host = "https://fcs.text-plus.org/"
client = openapi_client.ApiClient(configuration)
api = openapi_client.SearchApi(client)
```

## Package name

This package is published as `textplus-openapi-python` on PyPI and installs as `openapi_client`.
