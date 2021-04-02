<template>
<div id='searchbar'>
  <input id='search-bar' @input="onSearch" class="form-control" type="text" placeholder="Search" aria-label="Search"/>
</div>
</template>

<script>
export default {
  computed: {
    searchQuery: String
  },
  methods: {
    onSearch () {
      this.$emit('input', document.getElementById('search-bar').value)
    }
  }
}
</script>

<style scoped>

</style>
