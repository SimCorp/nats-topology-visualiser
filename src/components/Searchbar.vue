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
    // This method runs every time an input is given to the search bar
    // Emits the search text so other components can access it
    onSearch () {
      this.$emit('input', document.getElementById('search-bar').value)
    }
  }
}
</script>

<style scoped>

</style>
