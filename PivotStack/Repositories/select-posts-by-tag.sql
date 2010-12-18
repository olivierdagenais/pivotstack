SELECT
    pt.PostId
FROM
    PostTags AS pt
WHERE
    (pt.TagId = @tagId)
