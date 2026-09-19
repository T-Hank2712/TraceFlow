# OpenSearch Notes

## Dùng cho

- Full-text search.
- Filter theo keyword.
- Time range query.
- Aggregation cho operational insights nếu bật.
- Bulk indexing.

## Rủi ro

- Mapping explosion từ metadata.
- Shard quá nhiều hoặc quá ít.
- Query thiếu tenant filter.
- Deep pagination.
- Indexing pressure khi batch quá lớn.

## Cần benchmark

- Bulk size.
- Refresh interval.
- Query latency theo time range.
- Operational insights aggregation cost.
