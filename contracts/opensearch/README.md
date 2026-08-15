# OpenSearch Contracts

Thư mục này chứa mapping và cấu trúc document được index vào OpenSearch.

OpenSearch contract quyết định cách log được lưu, tìm kiếm, filter, sort và aggregate. Mapping cần bảo vệ các query chính như search theo message, filter theo project/service/level/environment, time range và trace lookup.

Mọi thay đổi mapping phải cân nhắc migration, reindexing và compatibility với Search API.
